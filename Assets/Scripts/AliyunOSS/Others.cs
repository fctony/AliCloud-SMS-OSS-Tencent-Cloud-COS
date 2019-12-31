using Aliyun.OSS;
using Aliyun.OSS.Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

public class Others : MonoBehaviour
{
    public static Others Instance;
    private OssClient client;

    private void Awake()
    {
        Instance = this;
        client = new OssClient(Config.EndPoint, Config.AccessKeyId, Config.AccessKeySecret);
    }
    /// <summary>
    /// 获取所有的名字和后缀
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllFileName()
    {
        ObjectListing list = client.ListObjects(Config.Bucket);
        List<string> nameList = new List<string>();

        foreach (var item in list.ObjectSummaries)
        {
            if (Regex.IsMatch(item.Key, "/") == false)
            {
                nameList.Add(item.Key);
            }
        }
        return nameList;
    }
    /// <summary>
    /// 创建空文件夹
    /// </summary>
    public void CreatEmptyFloder(string floderName)
    {
        try
        {
            using (var stream = new MemoryStream())
            {
                client.PutObject(Config.Bucket, floderName + "/", stream);
            }
        }
        catch (OssException e)
        {
            Debug.Log("创建文件夹出错：" + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.Log("创建文件夹出错：" + e.Message);
        }
    }
    /// <summary>
    /// 删除单个文件
    /// </summary>
    /// <param name="filePath"></param>
    public void DeleteObj(string filePath)
    {
        try
        {
            client.DeleteObject(Config.Bucket, filePath);
        }
        catch (OssException e)
        {
            Debug.Log("删除文件出错：" + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.Log("删除文件出错：" + e.Message);
        }
    }
    /// <summary>
    /// 删除多个文件
    /// </summary>
    /// <param name="filePathArr"></param>
    public void DeleteObjs(params string[] filePathArr)
    {
        try
        {
            List<string> filePathList = filePathArr.ToList();
            DeleteObjectsRequest deleteObjectsRequest = new DeleteObjectsRequest(Config.Bucket, filePathList);
            client.DeleteObjects(deleteObjectsRequest);
        }
        catch (OssException e)
        {
            Debug.Log("删除多个文件出错：" + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.Log("删除多个文件出错：" + e.Message);
        }
    }
    public void PictureHandle(string filePath, string savePath)
    {
        try
        {
            string process = "image/auto-orient,1/quality,q_90/bright,60/sharpen,100/blur,r_5,s_5/watermark,text_U0lLaeWtpumZog,size_40,shadow_100,x_10,y_10";
            GetObjectRequest getObjectRequest = new GetObjectRequest(Config.Bucket, filePath, process);
            OssObject result = client.GetObject(getObjectRequest);
            using (var resultStream = result.Content)
            {
                using (var fs = File.Open(savePath, FileMode.OpenOrCreate))
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
            Debug.Log("图片处理出错：" + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.Log("图片处理出错：" + e.Message);
        }
    }
}
