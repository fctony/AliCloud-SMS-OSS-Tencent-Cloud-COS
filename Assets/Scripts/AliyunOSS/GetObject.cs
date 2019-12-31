using Aliyun.OSS;
using Aliyun.OSS.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class GetObject : MonoBehaviour
{
    public static GetObject Instance;
    private OssClient client;
    private Thread getThread;
    private Action GetSuccessCallBack;
    private Action<float> GetObjectProcessCallBack;
    private string FilePath;
    private string SavePath;
    private bool isGetSuccess = false;
    private float process = 0.0f;
    private WWW wwwOpration;
    private UnityWebRequestAsyncOperation asyncOperation;

    private void Awake()
    {
        Instance = this;
        client = new OssClient(Config.EndPoint, Config.AccessKeyId, Config.AccessKeySecret);
    }
    private void Update()
    {
        if (isGetSuccess)
        {
            GetSuccessCallBack();
            isGetSuccess = false;
        }
        if (GetObjectProcessCallBack != null)
        {
            GetObjectProcessCallBack(process);
            if (process == 1)
            {
                GetObjectProcessCallBack = null;
                process = 0.0f;
            }
        }
        if (wwwOpration != null && wwwOpration.isDone)
        {
            byte[] bytes = wwwOpration.bytes;
            wwwOpration = null;
            File.WriteAllBytes(SavePath, bytes);
        }
        if (wwwOpration != null)
        {
            GetObjectProcessCallBack(wwwOpration.progress);
        }

        if (asyncOperation != null && asyncOperation.isDone)
        {
            byte[] bytes = asyncOperation.webRequest.downloadHandler.data;
            asyncOperation = null;
            File.WriteAllBytes(SavePath, bytes);
        }
        if (asyncOperation != null)
        {
            GetObjectProcessCallBack(asyncOperation.progress);
        }
    }
    /// <summary>
    /// 简单的文件下载
    /// </summary>
    /// <param name="action"></param>
    /// <param name="filePath"></param>
    /// <param name="savePath"></param>
    public void GetObjectSimple(Action action, string filePath, string savePath)
    {
        FilePath = filePath;
        SavePath = savePath;
        GetSuccessCallBack = action;
        getThread = new Thread(GetObjectSimple);
        getThread.Start();
    }
    private void GetObjectSimple()
    {
        try
        {
            OssObject result = client.GetObject(Config.Bucket, FilePath);
            using (var resultStream = result.Content)
            {
                using (var fs = File.Open(SavePath, FileMode.OpenOrCreate))
                {
                    int length = (int)resultStream.Length;
                    byte[] bytes = new byte[length];
                    do
                    {
                        length = resultStream.Read(bytes, 0, length);
                        fs.Write(bytes, 0, length);
                    } while (length != 0);
                }
            }
        }
        catch (OssException e)
        {
            Debug.Log("简单文件下载出错：" + e);
        }
        catch (System.Exception e)
        {
            Debug.Log("简单文件下载出错：" + e);
        }
        finally
        {
            getThread.Abort();
            isGetSuccess = true;
        }
    }
    public void GetObjectProcess(Action<float> action, string filePath, string savePath)
    {
        GetObjectProcessCallBack = action;
        FilePath = filePath;
        SavePath = savePath;
        getThread = new Thread(GetObjectProcess);
        getThread.Start();
    }
    private void GetObjectProcess()
    {
        try
        {
            GetObjectRequest getObjectRequest = new GetObjectRequest(Config.Bucket, FilePath);
            getObjectRequest.StreamTransferProgress += StreamProcess;
            OssObject result = client.GetObject(getObjectRequest);
            using (var resultStream = result.Content)
            {
                using (var fs = File.Open(SavePath, FileMode.OpenOrCreate))
                {
                    int length = (int)resultStream.Length;
                    byte[] bytes = new byte[length];
                    do
                    {
                        length = resultStream.Read(bytes, 0, length);
                        fs.Write(bytes, 0, length);
                    } while (length != 0);
                }
            }
            Debug.Log("带有进度下载文件成功");
        }
        catch (OssException e)
        {
            Debug.Log("带有进度文件下载出错：" + e.Message);
        }
        catch (Exception e)
        {
            Debug.Log("带有进度文件下载出错：" + e.Message);
        }
        finally
        {
            getThread.Abort();
        }
    }
    private void StreamProcess(object sender, StreamTransferProgressArgs args)
    {
        process = (args.TransferredBytes * 100 / args.TotalBytes) / 100.0f;
    }
    /// <summary>
    /// 使用WWW进行下载文件
    /// </summary>
    /// <param name="action"></param>
    /// <param name="filePath"></param>
    /// <param name="savePath"></param>
    public void GetObjectWithWWW(Action<float> action, string filePath, string savePath)
    {
        GetObjectProcessCallBack = action;
        SavePath = savePath;
        StartCoroutine(GetObjectWithWWW(filePath));
    }
    private IEnumerator GetObjectWithWWW(string filePath)
    {
        string path = "https://" + Config.Bucket + "." + Config.EndPoint + "/" + filePath;
        wwwOpration = new WWW(path);
        yield return wwwOpration;
    }
    /// <summary>
    /// 使用UnityWebRequest方式下载文件
    /// </summary>
    public void GetObjectWithUnityWebRequest(Action<float> action, string filePath, string savePath)
    {
        GetObjectProcessCallBack = action;
        SavePath = savePath;
        StartCoroutine(GetObjectWithUnityWebRequest(filePath));
    }
    private IEnumerator GetObjectWithUnityWebRequest(string filePath)
    {
        string path = "https://" + Config.Bucket + "." + Config.EndPoint + "/" + filePath;
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(path);
        asyncOperation = unityWebRequest.SendWebRequest();
        yield return asyncOperation;
    }
}
