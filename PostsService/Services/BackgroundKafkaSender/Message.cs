using Microsoft.AspNetCore.SignalR;
using PostsService.Entities;

namespace PostsService.Services.BackgroundKafkaSender
{
    public class Message<T> where T : class, ISerializableObject
    {
        public string header { get; set; }
        public T body { get; set; }

        public string SerializeToJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
