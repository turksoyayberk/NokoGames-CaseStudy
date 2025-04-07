using Game.AI;
using Game.Common;
using Zenject;
using Game.Storage;
using Pool;

namespace Core
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<RawStorage>().FromComponentInHierarchy().AsSingle();
            Container.Bind<SoundManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ProcessingStorage>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ProcessedStorage>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ItemPool>().FromComponentInHierarchy().AsSingle();
            Container.Bind<RawSpawner>().FromComponentInHierarchy().AsSingle();
            Container.Bind<AIManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}