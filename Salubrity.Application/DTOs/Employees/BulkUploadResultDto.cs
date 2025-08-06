public class BulkUploadResultDto
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkUploadError> Errors { get; set; } = [];
}

public class BulkUploadError
{
    public int Row { get; set; }
    public string Message { get; set; } = string.Empty;
}
