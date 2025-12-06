using Fusion;
using UnityEngine;

public class FusionLauncher : MonoBehaviour
{
    public static NetworkRunner runner; // Tek global runner

    private void Awake()
    {
        // Eğer zaten var ise yenisini oluşturma
        if (runner != null)
        {
            Destroy(gameObject);
            return;
        }

        //  Önce kendimizi kalıcı yapalım
        DontDestroyOnLoad(gameObject);

        //  Sonra runner'ı oluştur
        runner = gameObject.GetComponent<NetworkRunner>();
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            Debug.Log("Yeni NetworkRunner oluşturuldu.");
        }

        runner.ProvideInput = true;

        //  runner oluşturulduktan SONRA scene manager ekle
        var sceneManager = runner.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            Debug.Log("NetworkSceneManagerDefault eklendi.");
        }

        Debug.Log("Fusion runner başlatıldı (singleton).");
    }

    private void OnApplicationQuit()
    {
        if (runner != null && runner.IsRunning)
        {
            runner.Shutdown();
            Debug.Log("Fusion runner kapatıldı.");
        }
    }
}