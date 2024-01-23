namespace PostsService.Exceptions
{
    public class NoSuchRecordInDbException:Exception
    {
        public NoSuchRecordInDbException(string message) : base(message) { }
    }
}
