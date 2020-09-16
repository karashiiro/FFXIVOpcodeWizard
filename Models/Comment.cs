namespace FFXIVOpcodeWizard.Models
{
    public class Comment
    {
        public string Text { get; set; }

        public static implicit operator string(Comment c) => c.Text;
    }
}