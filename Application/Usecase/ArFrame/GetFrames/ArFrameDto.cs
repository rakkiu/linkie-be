namespace Application.Usecase.ArFrame.GetFrames
{
    public class ArFrameDto
    {
        public Guid Id { get; set; }
        public string FrameName { get; set; } = string.Empty;
        public string FrameUrl { get; set; } = string.Empty;
    }
}
