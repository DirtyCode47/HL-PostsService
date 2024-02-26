namespace PostsService.Entities
{
    public interface ISerializableObject
    {
        public string Serialize()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
