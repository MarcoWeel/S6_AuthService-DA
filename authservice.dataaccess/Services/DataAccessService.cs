using authservice.dataaccess.Data;
using authservice.dataaccess.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace authservice.dataaccess.Services;

public interface IDataAccessService
{
    void SubscribeToPersistence();
}

public class DataAccessService : IDataAccessService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessagingService _messagingService;

    public DataAccessService(IServiceProvider serviceProvider, IMessagingService messagingService)
    {
        _serviceProvider = serviceProvider;
        _messagingService = messagingService;

    }

    public void SubscribeToPersistence()
    {
        _messagingService.Subscribe("auth-data", (BasicDeliverEventArgs ea, string queue, string request) => RouteCallback(ea, queue, request), ExchangeType.Topic, "*.*.request");
    }

    private async void RouteCallback(BasicDeliverEventArgs ea, string queue, string request)
    {
        using AuthContext context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AuthContext>();

        string route = ea.RoutingKey.Replace("request", "response");

        string data = Encoding.UTF8.GetString(ea.Body.ToArray());
        string exchange = ea.Exchange;

        switch (request)
        {
            case "getbyemail":
                {
                    var user = await context.User.SingleOrDefaultAsync(m => m.Email == data);
                    var json = JsonConvert.SerializeObject(user);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "getbyid":
                {
                    Guid id = Guid.Parse(data);
                    var user = await context.User.SingleOrDefaultAsync(m => m.Id == id);
                    var json = JsonConvert.SerializeObject(user);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "adduser":
                {
                    var user = JsonConvert.DeserializeObject<User>(data);
                    if (user == null)
                        break;

                    context.Add(user);
                    await context.SaveChangesAsync();

                    var newUser = await context.User.SingleOrDefaultAsync(m => m.Email == user.Email);
                    if (newUser == null)
                        break;
                    var json = JsonConvert.SerializeObject(newUser);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "updateuser":
                {
                    var updateduser = JsonConvert.DeserializeObject<User>(data);
                    if (updateduser == null)
                        break;

                    var olduser = await context.User.SingleOrDefaultAsync(m => m.Id == updateduser.Id);
                    if (olduser == null)
                        break;

                    olduser.Email = updateduser.Email;
                    olduser.Username = updateduser.Username;
                    olduser.PasswordHash = updateduser.PasswordHash;
                    olduser.PhoneNumber = updateduser.PhoneNumber;
                    olduser.Acknowledged = updateduser.Acknowledged;
                    olduser.Roles = updateduser.Roles;

                    await context.SaveChangesAsync();

                    var json = JsonConvert.SerializeObject(updateduser);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "deleteuser":
                {
                    Guid id = Guid.Parse(data);

                    var user = await context.User.SingleOrDefaultAsync(m => m.Id == id);
                    if (user == null)
                        return;

                    context.User.Remove(user);
                    await context.SaveChangesAsync();
                    var json = JsonConvert.SerializeObject(user);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "getallusers":
                {
                    List<User> users = await context.User.ToListAsync();
                    var json = JsonConvert.SerializeObject(users);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                };
            default:
                Console.WriteLine($"Request {request} Not Found");
                break;
        }
    }
}
