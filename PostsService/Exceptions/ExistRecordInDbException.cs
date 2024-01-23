namespace PostsService.Exceptions
{
    public class ExistRecordInDbException:Exception
    {
        public ExistRecordInDbException(string message) : base(message) { }
    }
}
