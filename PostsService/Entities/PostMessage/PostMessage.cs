using System.ComponentModel.DataAnnotations;

namespace PostsService.Entities.PostMessage
{
    public class PostMessage : IPosts,ISerializableObject
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(5)]
        public string Code { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string River { get; set; }
        [Required]
        public PostStatus postStatus { get; set; }
    }
}
