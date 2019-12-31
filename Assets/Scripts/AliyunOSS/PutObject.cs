using Aliyun.OSS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using Aliyun.OSS.Common;
using System.Threading;
using System;

public class PutObject : MonoBehaviour
{
    public static PutObject Instance;

    private OssClient client;
    private Thread thread;
    private string localPath;
    private string fileName;
    private Action PutSuccessCallBack;
    private Action<float> PutWithProcessCallBack = null;
    private float putProcess;
    private bool isPutSuccess = false;

    private void Awake()
    {
        Instance = this;
        client = new OssClient(Config.EndPoint, Config.AccessKeyId, Config.AccessKeySecret);
    }
    //字符串上传
    public void PutObjWithStr(string fileName, string text)
    {
        try
        {
            byte[] b = Encoding.UTF8.GetBytes(text);
            using (Stream stream = new MemoryStream(b))
            {
                client.PutObject(Config.Bucket, fileName, stream);
                Debug.Log("字符串上传成功：" + text);
            }
        }
        catch (OssException e)
        {
            Debug.Log("字符串上传错误：" + e);
        }
        catch (System.Exception e)
        {
            Debug.Log("字符串上传错误：" + e);
        }
    }
    public void PutObjectFromLocalThread(Action action, string localPath, string fileName)
    {
        this.localPath = localPath;
        this.fileName = fileName;
        PutSuccessCallBack = action;
        thread = new Thread(PutObjFromLocal);
        thread.Start();
    }
    //本地上传
    private void PutObjFromLocal()
    {
        try
        {
            client.PutObject(Config.Bucket, fileName, localPath);
            Debug.Log("本地上传成功：" + fileName);
            isPutSuccess = true;
        }
        catch (OssException e)
        {
            Debug.Log("本地上传报错：" + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.Log("本地上传报错：" + e.Message);
        }
        finally
        {
            thread.Abort();
        }
    }
    private void FixedUpdate()
    {
        if (PutWithProcessCallBack != null)
        {
            PutWithProcessCallBack(putProcess);
            if (putProcess == 1)//代表上传成功了
            {
                PutWithProcessCallBack = null;
                putProcess = 0;
            }
        }
        if (isPutSuccess)
        {
            PutSuccessCallBack();
            isPutSuccess = false;
        }
    }
    /// <summary>
    /// 带有进度条的上传
    /// </summary>
    /// <param name="action"></param>
    /// <param name="localPath"></param>
    /// <param name="fileName"></param>
    public void PutObjectWithProcess(Action<float> action, string localPath, string fileName)
    {
        PutWithProcessCallBack = action;
        this.localPath = localPath;
        this.fileName = fileName;
        thread = new Thread(PutObjectProcess);
        thread.Start();
    }
    private void PutObjectProcess()
    {
        try
        {
            using (var fs = File.Open(localPath, FileMode.Open))
            {
                PutObjectRequest putObjectRequest = new PutObjectRequest(Config.Bucket, fileName, fs);
                putObjectRequest.StreamTransferProgress += PutStreamProcess;
                client.PutObject(putObjectRequest);
                Debug.Log("带有进度的上传成功");
            }
        }
        catch (OssException e)
        {
            Debug.Log("带有进度的上传错误：" + e);
        }
        catch (Exception e)
        {
            Debug.Log("带有进度的上传错误：" + e);
        }
        finally
        {
            thread.Abort();
        }
    }
    private void PutStreamProcess(object sender, StreamTransferProgressArgs args)
    {
        putProcess = (args.TransferredBytes * 100 / args.TotalBytes) / 100.0f;
    }
}
