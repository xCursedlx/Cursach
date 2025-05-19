public class UserListItem
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }

    public override string ToString() => $"{Username} ({FullName})";
}
