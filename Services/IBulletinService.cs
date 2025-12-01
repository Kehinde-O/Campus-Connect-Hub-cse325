using DigitalBulletinBoard.Models;


public interface IBulletinService
{
Task<List<Bulletin>> GetAllAsync();
Task<Bulletin?> GetByIdAsync(string id);
Task CreateAsync(Bulletin b);
Task UpdateAsync(Bulletin b);
Task DeleteAsync(string id);
}