using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public static Test Instance;

    public Text hint;
    public GameObject go_SMS;
    private InputField input_Phone;
    private InputField input_VerCode;
    private Button btn_Send;
    private Button btn_Ok;
    private Text txt_CountDown;
    private string m_VerCode;
    private bool m_IsSend = false;
    private float timer = 0.0f;
    private int time = 60;

    private InputField input_LocalPath;
    private Button btn_Upload;
    private Image img_Process;
    private Button btn_Refresh;
    public GameObject go_Item;
    private Transform parent;

    private void Awake()
    {
        Instance = this;
        input_Phone = go_SMS.transform.Find("input_Phone").GetComponent<InputField>();
        input_VerCode = go_SMS.transform.Find("input_VerCode").GetComponent<InputField>();
        btn_Send = go_SMS.transform.Find("btn_Send").GetComponent<Button>();
        btn_Send.onClick.AddListener(OnSendButtonClick);
        btn_Ok = go_SMS.transform.Find("btn_Ok").GetComponent<Button>();
        btn_Ok.onClick.AddListener(OnOkButtonClick);
        txt_CountDown = go_SMS.transform.Find("txt_CountDown").GetComponent<Text>();
        txt_CountDown.gameObject.SetActive(false);
        input_LocalPath = transform.Find("OSS/input_LocalPath").GetComponent<InputField>();
        btn_Upload = transform.Find("OSS/btn_Upload").GetComponent<Button>();
        btn_Upload.onClick.AddListener(OnUploadButtonClick);
        img_Process = transform.Find("OSS/img_Process").GetComponent<Image>();
        btn_Refresh = transform.Find("btn_Refresh").GetComponent<Button>();
        btn_Refresh.onClick.AddListener(OnRefreshButtonClick);
        parent = transform.Find("list/ScrollRect/parent");
    }
    private void OnRefreshButtonClick()
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
        List<string> filaNameList = Others.Instance.GetAllFileName();
        for (int i = 0; i < filaNameList.Count; i++)
        {
            GameObject go = Instantiate(go_Item, parent);
            go.transform.GetChild(0).GetComponent<Text>().text = filaNameList[i];
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CosDemo.Instance.UploadObj("123.zip", @"C:\Users\山商艾欧特\Desktop\阿里云OSS、SMS、腾讯云COS课程素材\aliyun_oss_dotnet_sdk_2_8_0.zip");
            //Others.Instance.PictureHandle("签名申请模板1.jpg", @"C:\Users\山商艾欧特\Desktop\test.jpg");
            //Others.Instance.DeleteObj("123.txt");
            //Others.Instance.DeleteObjs("123.zip", "test.txt");
            //Others.Instance.CreatEmptyFloder("123");
            //foreach (var item in Others.Instance.GetAllFileName())
            //{
            //    Debug.Log(item);
            //}
            //string filePath = "123.zip";
            //GetObject.Instance.GetObjectWithUnityWebRequest(PutWithProcessCallBack, filePath, @"C:\Users\山商艾欧特\Desktop\" + filePath);
            //string filePath = "123.zip";
            //GetObject.Instance.GetObjectSimple(GetSuccessCallBack, filePath, @"C:\Users\山商艾欧特\Desktop\" + filePath);
            //PutObject.Instance.PutObjWithStr("test.txt", "dshdfwhh你好，我是Sikiedu");
            //PutObject.Instance.PutObjectWithProcess(PutWithProcessCallBack, @"C:\Users\山商艾欧特\Desktop\aliyun_oss_dotnet_sdk_2_8_0.zip", "123.zip");
        }
    }
    /// <summary>
    /// 上传按钮点击
    /// </summary>
    private void OnUploadButtonClick()
    {
        if (input_LocalPath.text == null || input_LocalPath.text == "") return;
        PutObject.Instance.PutObjectWithProcess(PutWithProcessCallBack, input_LocalPath.text, Path.GetFileName(input_LocalPath.text));
    }
    private void GetSuccessCallBack()
    {
        Debug.Log("下载成功");
    }
    public void PutWithProcessCallBack(float process)
    {
        img_Process.fillAmount = process;
        Debug.Log(process);
    }
    private void FixedUpdate()
    {
        if (m_IsSend)
        {
            if (time <= 0)
            {
                m_IsSend = false;
                time = 60;
                timer = 0.0f;
            }
            timer += Time.deltaTime;
            if (timer >= 1)
            {
                timer = 0.0f;
                time--;
                txt_CountDown.text = time.ToString() + "S";
            }
        }
    }
    private void OnSendButtonClick()
    {
        if (input_Phone.text == "" || input_Phone.text == null)
        {
            hint.gameObject.SetActive(true);
            hint.text = "请输入手机号";
            StartCoroutine(Dealy());
            //Debug.Log("请输入手机号");
            return;
        }
        string patten = @"^1\d{10}$";
        if (Regex.IsMatch(input_Phone.text, patten) == false)
        {
            hint.gameObject.SetActive(true);
            hint.text = "手机号格式错误";
            StartCoroutine(Dealy());
            //Debug.Log("手机号格式错误");
            return;
        }

        m_VerCode = Random.Range(111111, 999999).ToString();
        AliyunSMS.sendSms(input_Phone.text, m_VerCode);
        hint.gameObject.SetActive(true);
        hint.text = "短信发送成功";
        StartCoroutine(Dealy());
        //Debug.Log("短信发送成功");
        txt_CountDown.gameObject.SetActive(true);
        txt_CountDown.text = "60S";
        btn_Send.gameObject.SetActive(false);
        m_IsSend = true;
    }
    private void OnOkButtonClick()
    {
        if (input_VerCode.text == m_VerCode)
        {
            hint.gameObject.SetActive(true);
            hint.text = "验证码正确";
            StartCoroutine(Dealy());
            //Debug.Log("验证码正确");
        }
        else
        {
            hint.gameObject.SetActive(true);
            hint.text = "验证码错误";
            StartCoroutine(Dealy());
            //Debug.Log("验证码错误");
        }
    }
    IEnumerator Dealy()
    {
        yield return new WaitForSeconds(1.5f);
        hint.gameObject.SetActive(false);
    }
}
