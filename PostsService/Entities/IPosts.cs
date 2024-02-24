using System.ComponentModel.DataAnnotations;

namespace PostsService.Entities
{
    public interface IPosts
    {
        public Guid Id { get; set; } 
        public string Code { get; set; }
        public string Name { get; set; }
        public string River { get; set; }
    }
}
