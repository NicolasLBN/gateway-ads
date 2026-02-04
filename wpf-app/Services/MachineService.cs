using WpfApp.Models;

namespace WpfApp.Services;

public class MachineService
{
    private readonly List<Machine> _machines = new()
    {
        new Machine
        {
            Id = "1",
            Name = "Mixing Unit A",
            AmsNetId = "127.0.0.1.1.1",
            AmsPort = 851,
            Description = "Primary mixing unit for standard recipes"
        },
        new Machine
        {
            Id = "2",
            Name = "Mixing Unit B",
            AmsNetId = "127.0.0.1.1.1",
            AmsPort = 852,
            Description = "Secondary mixing unit for specialized recipes"
        },
        new Machine
        {
            Id = "3",
            Name = "Mixing Unit C",
            AmsNetId = "127.0.0.1.1.1",
            AmsPort = 853,
            Description = "High-capacity mixing unit"
        }
    };

    public List<Machine> GetMachines()
    {
        return _machines;
    }

    public Machine? GetMachine(string id)
    {
        return _machines.FirstOrDefault(m => m.Id == id);
    }
}
