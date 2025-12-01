using DigitalBulletinBoard.Models;


public class InMemoryBulletinService : IBulletinService
{
private readonly List<Bulletin> _items = new();


public InMemoryBulletinService()
{
// seed example
_items.Add(new Bulletin{ Title = "Welcome Week", Content = "Join orientation...", Author = "Admin", CreatedAt = DateTime.UtcNow });
_items.Add(new Bulletin{ Title = "Math Club", Content = "Meeting Friday 5pm", Author = "Alice", CreatedAt = DateTime.UtcNow.AddDays(-1)});
}


public Task<List<Bulletin>> GetAllAsync() => Task.FromResult(_items.OrderByDescending(b => b.CreatedAt).ToList());


public Task<Bulletin?> GetByIdAsync(string id) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));


public Task CreateAsync(Bulletin b)
{
b.Id = Guid.NewGuid().ToString();
b.CreatedAt = DateTime.UtcNow;
_items.Add(b);
return Task.CompletedTask;
}


public Task UpdateAsync(Bulletin b)
{
var idx = _items.FindIndex(x => x.Id == b.Id);
if (idx >= 0) _items[idx] = b;
return Task.CompletedTask;
}


public Task DeleteAsync(string id)
{
var item = _items.FirstOrDefault(x => x.Id == id);
if (item != null) _items.Remove(item);
return Task.CompletedTask;
}
}