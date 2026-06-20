namespace YoungJoon.L1.App
{
    /// <summary> 특정 씬을 관장하는 매니저 </summary>
    public interface ISceneManager
    {
        string SceneName { get; }
        void OnEnterScene(SceneContext context);
        void OnExitScene();
    }
}
