namespace jkerak.emailsender.Model
{
    public class UnsubscribeRequest
    {
        public int PersonId { get; set; }
        public string UseCase { get; set; }
        public string Signature { get; set; }
    }
}
