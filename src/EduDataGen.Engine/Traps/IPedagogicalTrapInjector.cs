namespace EduDataGen.Engine.Traps;

using EduDataGen.DAL.Models;

public interface IPedagogicalTrapInjector
{
    List<Dictionary<string, string?>> InjectTraps(
        List<Dictionary<string, string?>> records,
        AnomalySettings settings,
        int seed = 2026);
}
