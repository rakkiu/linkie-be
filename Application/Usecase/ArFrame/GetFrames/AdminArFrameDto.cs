namespace Application.Usecase.ArFrame.GetFrames
{
    /// <summary>
    /// Extended DTO for admin view – includes IsActive status.
    /// </summary>
    public class AdminArFrameDto
    {
        public Guid Id { get; set; }
        public string FrameName { get; set; } = string.Empty;
        public string FrameUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
