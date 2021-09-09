using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenPop;
using OpenPop.Pop3;
using System.IO;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using Newtonsoft.Json;
using SharpAdbClient;
using ImapX;
using OpenQA.Selenium.Interactions;

namespace Twitter_Account_Creator
{
    public partial class Form1 : MaterialSkin.Controls.MaterialForm
    {
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        public class Kayit
        {
            public string counter { get; set; }
            public string counter_formail { get; set; }
        }

        IWebDriver WD;

        string[] e_isimler;
        string[] k_isimler;
        string[] u_agents;
        string[] mailandpw;
        string[] cache_mails;
        string[] bios;
        string[] f_tweets;
        string[] konumlar;
        string[] sites;
        string[] takip_edilecekler;
        Kayit kayitlar;

        int counter = 0;
        int counter_formail = 0;
        List<Thread> thread_Pool = new List<Thread>();

        AdbServer AS;
        DeviceData mDevice;

       

        public void HoverMouseOverGO(IWebElement element)
        {
            Actions actions = new Actions(WD);
            actions.MoveToElement(element);
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)WD;
            javaScriptExecutor.ExecuteScript("arguments[0].scrollIntoView(true);", new object[]
            {
        element
            });
        }

        public void HoverMouseOverinput(IWebElement element, string send)
        {
            Actions actions = new Actions(WD);
            actions.MoveToElement(element).Build().Perform();
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)WD;
            javaScriptExecutor.ExecuteScript("arguments[0].focus();", new object[]
            {
        element
            });
            element.SendKeys(send);
        }

        public void HoverMouseOverClick(IWebElement element)
        {
            Actions actions = new Actions(WD);
            actions.MoveToElement(element).Perform();
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)WD;
            javaScriptExecutor.ExecuteScript("arguments[0].scrollIntoView(true);arguments[0].click();", new object[]
            {
        element
            });
        }

        public bool IsTestElementPresent(By element)
        {
            bool result;
            try
            {
                WD.FindElement(element);
                result = true;
            }
            catch (NoSuchElementException)
            {
                result = false;
            }
            return result;
        }

        public bool IsElementDisplayed(By element)
        {
            bool result;
            try
            {
                bool displayed = WD.FindElement(element).Displayed;
                bool flag = displayed;
                result = flag;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public bool GecmeButonuTikla()
        {
            if (IsElementDisplayed(By.XPath("//div[@role='button']/div[@dir='auto']/span/span[contains(text(),'Next') or contains(text(),'İleri') or contains(text(),'Sign up') or contains(text(),'Kaydol') or contains(text(),'Şimdilik geç') or contains(text(),'Skip for now') or contains(text(),'Not Now') or contains(text(),'Şimdi değil') or contains(text(),'Takip sayısı:') or contains(text(),'OK') or contains(text(),'Apply') or contains(text(),'Onayla') or contains(text(),'Bildirimlere izin ver') or contains(text(),'#Hayır') or contains(text(),'Hayır, teşekkürler')]")))
            {
                HoverMouseOverClick(WD.FindElement(By.XPath("//div[@role='button']/div[@dir='auto']/span/span[contains(text(),'Next') or contains(text(),'İleri') or contains(text(),'Sign up') or contains(text(),'Kaydol') or contains(text(),'Şimdilik geç') or contains(text(),'Skip for now') or contains(text(),'Not Now') or contains(text(),'Şimdi değil') or contains(text(),'Takip sayısı:') or contains(text(),'OK') or contains(text(),'Apply') or contains(text(),'Onayla') or contains(text(),'Bildirimlere izin ver') or contains(text(),'#Hayır') or contains(text(),'Hayır, teşekkürler')]")));
                return true;
            }
            else
                return false;
        }


        private void UcakModuAc(DeviceData DD)
        {
            AdbClient.Instance.ExecuteRemoteCommand("su -c svc data disable", DD, null);

            //AdbClient.Instance.ExecuteRemoteCommand("su -c settings put global airplane_mode_on 1", DD, null);
            //AdbClient.Instance.ExecuteRemoteCommand("su -c am broadcast -a android.intent.action.AIRPLANE_MODE --ez state true", DD, null);
        }

        private void UcakModuKapat(DeviceData DD)
        {
            AdbClient.Instance.ExecuteRemoteCommand("su -c svc data enable", DD, null);
            //AdbClient.Instance.ExecuteRemoteCommand("su -c settings put global airplane_mode_on 0", DD, null);
            //AdbClient.Instance.ExecuteRemoteCommand("su -c am broadcast -a android.intent.action.AIRPLANE_MODE --ez state false", DD, null);
        }

        private IWebElement WaitForUse(By locator)
        {
            Thread.Sleep(500);
            try
            {
                if (WD.FindElement(locator).Displayed && WD.FindElement(locator).Enabled)
                {
                    return WD.FindElement(locator);
                }
                else
                    return WaitForUse(locator);
            }
            catch
            {
                return WaitForUse(locator);
            }
        }




        protected void WaitForPageLoad()
        {
            WebDriverWait wait = new WebDriverWait(WD, TimeSpan.FromSeconds(10));
            wait.Until(WD => ((IJavaScriptExecutor)WD).ExecuteScript("return (document.readyState == 'complete')"));
        }

        private string CreateRandomPassword(int length = 10)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }

        private string GetActivationCodeFromMail(string mail, string pw)
        {

            var client = new ImapClient("imap." + mail.Split('@')[1], true);

            if (client.Connect())
            {

                if (client.Login(mail, pw))
                {
                    string[] folderlist = { "Spam", "Inbox", "INBOX", "Main" };

                    List<string> existfolders = new List<string>();


                    foreach (string str in folderlist)
                    {
                        try
                        {
                            long f = client.Folders[str].Exists;
                            existfolders.Add(str);
                        }
                        catch(Exception)
                        {

                        }
                    }


                    DateTime biggest = DateTime.Now - TimeSpan.FromDays(3);
                    string kod = "-1";

                    foreach (string s in existfolders)
                    {
                        ImapX.Message[] messagesx = client.Folders[s].Search("SUBJECT Twitter", ImapX.Enums.MessageFetchMode.Minimal);

                        if (messagesx.Length > 0)
                        {
                            if (messagesx[0].InternalDate >= biggest)
                            {
                                biggest = (DateTime)messagesx[0].InternalDate;
                                if (messagesx[0].Subject.Contains("Twitter onay kodun"))
                                {
                                    kod = messagesx[0].Subject.Replace("Twitter onay kodun ", "");
                                }else
                                {
                                    kod = new String(messagesx[0].Body.Text.Where(Char.IsDigit).ToArray()).Substring(0, 6);
                                }
                            }
                            //MessageBox.Show(messagesx[0].InternalDate.ToString());
                        }
                    }

                    return kod;



                }
                else
                {
                    MessageBox.Show("Maile giriş sağlanamadı!!!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "-1";
                }
            }
            else
            {
                MessageBox.Show("Mail sunucusuna bağlanılamadı!!!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "-1";
            }

            //using (Pop3Client client = new Pop3Client())
            //{
            //    // Connect to the server
            //    client.Connect("pop.mail.com", 995, true);

            //    // Authenticate ourselves towards the server
            //    //client.Authenticate("xxx", "xxx");
            //    client.Authenticate(mail, pw);

            //    // Get the number of messages in the inbox
            //    int messageCount = client.GetMessageCount();

            //    //// We want to download all messages
            //    //List<OpenPop.Mime.Message> allMessages = new List<OpenPop.Mime.Message>(messageCount);

            //    //// Messages are numbered in the interval: [1, messageCount]
            //    //// Ergo: message numbers are 1-based.
            //    //// Most servers give the latest message the highest number
            //    //for (int i = messageCount; i > 0; i--)
            //    //{
            //    //    allMessages.Add(client.GetMessage(i));
            //    //}
            //    OpenPop.Mime.Message msg = client.GetMessage(messageCount);


            //    if (msg.ToMailMessage().Subject.Contains("Twitter onay kodun"))
            //    {
            //        string kod = msg.ToMailMessage().Subject.Replace("Twitter onay kodun ", "");
            //       // MessageBox.Show(msg.ToMailMessage().Subject.Replace("Twitter onay kodun ", ""));
            //        return kod;
            //        //DateTime dt = DateTime.Parse(allMessages[0].Headers.Date);
            //        //dt.AddHours(3);
            //        //MessageBox.Show((dt.Ticks / 1000).ToString());
            //        //MessageBox.Show((DateTime.Now.Ticks / 1000).ToString());
            //    }
            //    else
            //        return "-1";

            //    // Now return the fetched messages

            //}
        }

        private void LoadFiles()
        {
            if (File.Exists(Application.StartupPath + "\\Erkek İsimleri.txt"))
                e_isimler = File.ReadAllLines(Application.StartupPath + "\\Erkek İsimleri.txt");
            if (File.Exists(Application.StartupPath + "\\Kadın İsimleri.txt"))
                k_isimler = File.ReadAllLines(Application.StartupPath + "\\Kadın İsimleri.txt");
            if (File.Exists(Application.StartupPath + "\\User Agent.txt"))
                u_agents = File.ReadAllLines(Application.StartupPath + "\\User Agent.txt");
            if (File.Exists(Application.StartupPath + "\\Mail Listesi.txt"))
                mailandpw = File.ReadAllLines(Application.StartupPath + "\\Mail Listesi.txt");
            if (File.Exists(Application.StartupPath + "\\bio.txt"))
                bios = File.ReadAllLines(Application.StartupPath + "\\bio.txt");
            if (File.Exists(Application.StartupPath + "\\ilk tweet .txt"))
                f_tweets = File.ReadAllLines(Application.StartupPath + "\\ilk tweet .txt");
            if (File.Exists(Application.StartupPath + "\\konum.txt"))
                konumlar = File.ReadAllLines(Application.StartupPath + "\\konum.txt");
            if (File.Exists(Application.StartupPath + "\\web sitesi listesi.txt"))
                sites = File.ReadAllLines(Application.StartupPath + "\\web sitesi listesi.txt");
            if (File.Exists(Application.StartupPath + "\\takip listesi.txt"))
                takip_edilecekler = File.ReadAllLines(Application.StartupPath + "\\takip listesi.txt");
            if (File.Exists(Application.StartupPath + "\\save.svx"))
            {
                kayitlar = JsonConvert.DeserializeObject<Kayit>(File.ReadAllText(Application.StartupPath + "\\save.svx"));

                counter = Convert.ToInt32(kayitlar.counter);
                counter_formail = Convert.ToInt32(kayitlar.counter_formail);

                lbl_cachecounter.Text = counter.ToString();
                lbl_normalcounter.Text = counter_formail.ToString();
            }
            

        }

        private string GetRandomImageFile()
        {
            var rand = new Random();
            var files = Directory.GetFiles(Application.StartupPath + "\\imgs", "*.jpg");
            return files[rand.Next(files.Length)];
        }

        private string GetRandomWallpaperFile()
        {
            var rand = new Random();
            var files = Directory.GetFiles(Application.StartupPath + "\\wallpapers", "*.jpg");
            return files[rand.Next(files.Length)];
        }

        private string GetRandomBio()
        {
            var rand = new Random();
            return bios[rand.Next(bios.Length)];
        }

        private string GetRandomKonum()
        {
            var rand = new Random();
            return konumlar[rand.Next(konumlar.Length)];
        }

        private string GetRandomSite()
        {
            var rand = new Random();
            return sites[rand.Next(sites.Length)];
        }

        private string GetRandomTweet()
        {
            var rand = new Random();
            return f_tweets[rand.Next(f_tweets.Length)];
        }

        private int GetRandomSleepTime(int min, int max)
        {
            var rand = new Random();
            return rand.Next(min * 1000, max * 1000);
        }

        private void ControlBrowser()
        {
                if (File.Exists(Application.StartupPath + "\\Mail_Cache.txt"))
                {
                    cache_mails = File.ReadAllLines(Application.StartupPath + "\\Mail_Cache.txt");
                }
                else
                {
                    OpenSubMails(mailandpw[counter_formail].Split(':')[0], mailandpw[counter_formail].Split(':')[1]);
                    cache_mails = File.ReadAllLines(Application.StartupPath + "\\Mail_Cache.txt");
                }

                if (counter == cache_mails.Length)
                {
                    //MessageBox.Show("Cache mail listesi sona erdi...");
                    string output = JsonConvert.SerializeObject(new Kayit { counter = counter.ToString(), counter_formail = counter_formail.ToString() });
                    if (File.Exists(Application.StartupPath + "\\save.svx"))
                        File.Delete(Application.StartupPath + "\\save.svx");

                    File.WriteAllText(Application.StartupPath + "\\save.svx", output);

                    ++counter_formail;
                    if (counter_formail >= mailandpw.Length)
                    {
                       // MessageBox.Show(mailandpw.Length.ToString() + " - " + counter_formail.ToString());
                        MessageBox.Show("Mail listesi sona erdi, lütfen mail listesini yeniledikten sonra programı kapatıp açınız.", "Uyarı!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                        return;
                    }
                        

                    OpenSubMails(mailandpw[counter_formail].Split(':')[0], mailandpw[counter_formail].Split(':')[1]);
                    ControlBrowser();
                    return;
                }
            

            Thread.Sleep(GetRandomSleepTime(3,4));    

            ChromeOptions CO = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            Random rnd = new Random();
            int randomum = rnd.Next(0, u_agents.Length - 1);

            CO.AddArgument("--user-agent=\"" + u_agents[randomum] + "\"");
            CO.AddArgument("--window-size=1300,1000");
            CO.AddArgument("incognito");
            // CO.AddArgument("headless"); // chrome gizleme

            WD = new ChromeDriver(service, CO);
            WD.Navigate().GoToUrl("https://twitter.com/i/flow/signup");

            Thread.Sleep(GetRandomSleepTime(3,4));

            WaitForPageLoad();

            

            

            int stage = 0;
            int hata_sayaci = 0;
            IWebElement ileri_button = null;
            string randompass = "";

            while (true)
            {
                Thread.Sleep(500);
                try
                {
                    if (WD.Url.Contains("twitter.com/i/flow/signup"))
                    {
                        if (stage == 0)
                        {
                            Thread.Sleep(1000);
                            try
                            {
                                hata_sayaci = 0;
                                Random rnd2 = new Random();
                                int K_E = rnd2.Next(0, 1);

                                Random rnd3 = new Random();
                                int rndfork = rnd3.Next(0, k_isimler.Length - 1);

                                Random rnd4 = new Random();
                                int rndfore = rnd4.Next(0, e_isimler.Length - 1);

                                if (K_E == 0)
                                {
                                    WebDriverWait wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                                    wdw.Until(ExpectedConditions.ElementIsVisible(By.Name("name"))).SendKeys(k_isimler[rndfork]);
                                }
                                else
                                {
                                    WebDriverWait wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                                    wdw.Until(ExpectedConditions.ElementIsVisible(By.Name("name"))).SendKeys(e_isimler[rndfore]);
                                }

                            }
                            catch(Exception)
                            {
                                continue;
                            }


                            if (IsTestElementPresent(By.XPath("//div[@dir='auto' and @role='button']"))){
                                HoverMouseOverClick(WD.FindElement(By.XPath("//div[@dir='auto' and @role='button']")));
                            }
                            Thread.Sleep(GetRandomSleepTime(1, 2));

                            if (IsTestElementPresent(By.Name("email")))
                            {
                                WD.FindElement(By.Name("email")).SendKeys(cache_mails[counter].Split(':')[0]);
                                stage = 1;
                            }
                            
                            //WD.FindElement(By.Name("email")).SendKeys("asdafsaiafisdmxqxfk@mail.com");
                            
                        }

                        if (stage == 1)
                        {
                            if (GecmeButonuTikla())
                                stage = 2;
                        }

                        if (stage == 2)
                        {
                            //WaitForPageLoad();
                            Thread.Sleep(GetRandomSleepTime(2,3));

                            if (GecmeButonuTikla())
                                stage = 3;
                        }

                        if (stage == 3)
                        {
                            Thread.Sleep(GetRandomSleepTime(2, 3));
                            //WaitForPageLoad();

                            if (GecmeButonuTikla())
                                stage = 4;
                        }

                        if (stage == 4)
                        {
                            Thread.Sleep(GetRandomSleepTime(2, 3));
                            //WaitForPageLoad();

                            if (GecmeButonuTikla())
                                stage = 5;
                        }

                        if (stage == 5)
                        {
                            Thread.Sleep(GetRandomSleepTime(2, 3));
                            //WaitForPageLoad();

                            if (GecmeButonuTikla())
                                stage = 6; 
                        }
                        
                        if (stage == 6)
                        {
                            Thread.Sleep(7000);
                            // verfication_code
                            string getCode = GetActivationCodeFromMail(cache_mails[counter].Split(':')[0], cache_mails[counter].Split(':')[1]);

                            if (getCode.Length == 6)
                            {
                                try { WD.FindElement(By.Name("verfication_code")).SendKeys(getCode); }
                                catch(Exception)
                                {
                                    AndroidReset();
                                    WD.Close();
                                    WD.Quit();
                                    WD.Dispose();
                                    counter++;

                                    string output2 = JsonConvert.SerializeObject(new Kayit { counter = counter.ToString(), counter_formail = counter_formail.ToString() });
                                    if (File.Exists(Application.StartupPath + "\\save.svx"))
                                        File.Delete(Application.StartupPath + "\\save.svx");

                                    File.WriteAllText(Application.StartupPath + "\\save.svx", output2);


                                    stage = -1;
                                    break;

                                }
                                //MessageBox.Show(getCode);
                            }
                            else
                            {
                                hata_sayaci++;
                                if (hata_sayaci >= 4)
                                {
                                    WD.Navigate().GoToUrl("https://twitter.com/i/flow/signup");
                                    ileri_button = null;
                                    stage = 0;
                                    WaitForPageLoad();
                                }

                                continue;
                            }
                            hata_sayaci = 0;
                            Thread.Sleep(4500);
                            if (GecmeButonuTikla())
                                stage = 7;
                        }

                        if (stage == 7)
                        {
                            // Thread.Sleep(1500);
                            Thread.Sleep(GetRandomSleepTime(3, 4));
                            if (randompass == "")
                                randompass = CreateRandomPassword();

                            //WebDriverWait wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                            //wdw.Until(ExpectedConditions.ElementIsVisible(By.Name("password"))).SendKeys(randompass);
                            if (IsTestElementPresent(By.Name("password")))
                            {
                                WD.FindElement(By.Name("password")).SendKeys(randompass);
                            }
                            //File.AppendAllText(Application.StartupPath + "\\passes.txt", randompass + "\r\n");
                            //WD.FindElement(By.Name("password"))

                            Thread.Sleep(GetRandomSleepTime(4,5));

                            if (GecmeButonuTikla())
                                stage = 8;


                        }

                        if (stage == 8)
                        {
                            Thread.Sleep(GetRandomSleepTime(4,5));


                            try
                            {
                                WD.FindElement(By.Id("phone_number"));
                                // Modem resle, mail değiş.
                                WD.Close();
                                WD.Quit();
                                WD.Dispose();
                                counter++;
                                AndroidReset();

                                string output2 = JsonConvert.SerializeObject(new Kayit { counter = counter.ToString(), counter_formail = counter_formail.ToString() });
                                if (File.Exists(Application.StartupPath + "\\save.svx"))
                                    File.Delete(Application.StartupPath + "\\save.svx");

                                File.WriteAllText(Application.StartupPath + "\\save.svx", output2);


                                lbl_cachecounter.Text = counter.ToString();

                                stage = -1;
                                break;
                            }
                            catch(Exception)
                            {

                            }

                            stage = 9;
                        }
                        //WebDriverWait wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                        //wdw.Until(ExpectedConditions.ElementExists(By.TagName("input")));

                        if (chk_profil_foto.Checked && stage == 9)
                        {
                            if (stage == 9)
                            {
                                Thread.Sleep(GetRandomSleepTime(3, 4));

                                IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("input"));

                                foreach (IWebElement we in welements2)
                                {
                                    if (we.GetAttribute("accept") == "image/jpeg,image/png,image/webp")
                                    {
                                        we.Clear();
                                        we.SendKeys(GetRandomImageFile());
                                        stage = 10;
                                        break;
                                    }
                                }
                            }
                        }
                        else if(!chk_profil_foto.Checked && stage == 9)
                        {
                            stage = 10;
                        }
                       

                        if (stage == 10)
                        {
                            Thread.Sleep(GetRandomSleepTime(3, 4));

                            if (GecmeButonuTikla())
                                stage = 11;
                        }
                        
                        if (chk_bio.Checked && stage == 11)
                        {
                            if (stage == 11)
                            {
                                Thread.Sleep(GetRandomSleepTime(3, 4));

                                //wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                                //wdw.Until(ExpectedConditions.ElementExists(By.Name("text"))).SendKeys(GetRandomBio());
                                try
                                {
                                    WD.FindElement(By.Name("text")).SendKeys(GetRandomBio());
                                    stage = 12;
                                }
                                catch (Exception ex)
                                {
                                    // MessageBox.Show("HATA BIO : \n" + ex.ToString());
                                }
                            }
                        }
                        else if(!chk_bio.Checked && stage == 11)
                        {
                            stage = 12;
                        }
                        

                        if (stage == 12)
                        {
                            Thread.Sleep(GetRandomSleepTime(3, 4));

                            if (GecmeButonuTikla())
                                stage = 13;
                        }
                           
                        if (stage == 13)
                        {
                            Thread.Sleep(GetRandomSleepTime(3, 4));


                            if (GecmeButonuTikla())
                                stage = 14;

                        }

                        if (stage == 14 && chk_oto_onerilen.Checked) {
                            if (stage == 14)
                            {
                                Thread.Sleep(GetRandomSleepTime(3, 4));


                                int sayac = 0;
                                int RandomSayac = new Random().Next(2, 6);
                                IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("span"));

                                foreach (IWebElement we in welements2)
                                {
                                    if (sayac >= RandomSayac)
                                    {
                                        stage = 15;
                                        break;
                                    }

                                    if (we.Text == "Takip et" || we.Text == "Follow")
                                    {
                                        we.Click();
                                        sayac++;
                                        Thread.Sleep(1000);
                                    }
                                }
                            }

                            if (stage == 15)
                            {
                                Thread.Sleep(GetRandomSleepTime(3, 4));

                                IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("span"));

                                foreach (IWebElement we in welements2)
                                {
                                    if (we.Text.Contains("Takip sayısı: "))
                                    {

                                        we.Click();
                                        stage = 16;
                                        break;
                                    }
                                }

                            }
                        }
                        else if (stage == 14 && !chk_oto_onerilen.Checked)
                        {
                            Thread.Sleep(GetRandomSleepTime(3, 4));


                            if (GecmeButonuTikla())
                                stage = 16;
                        }
                        

                        if (stage == 16)
                        {
                            Thread.Sleep(GetRandomSleepTime(3, 4));

                            WD.Navigate().GoToUrl("https://mobile.twitter.com/home");
                            WaitForPageLoad();
                            Thread.Sleep(GetRandomSleepTime(4, 5));
                            stage = 17;

                        }
                        
                            //counter++;

                            //string output = JsonConvert.SerializeObject(new Kayit { counter = counter.ToString(), counter_formail = counter_formail.ToString() });
                            //if (File.Exists(Application.StartupPath + "\\save.svx"))
                            //    File.Delete(Application.StartupPath + "\\save.svx");

                            //File.WriteAllText(Application.StartupPath + "\\save.svx", output);


                            //WD.Close();
                            //WD.Quit();
                            //WD.Dispose();

                            //AndroidReset();
                       
                        if (stage == 17)
                            {
                                IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("span"));

                                foreach (IWebElement we in welements2)
                                {
                                    if (we.Text == "Neler oluyor?" || we.Text == "What’s happening?")
                                    {
                                        we.Click();
                                        stage = 18;
                                        break;
                                    }
                                }
                            }
                        
                        if (stage == 18)
                            {
                                Thread.Sleep(GetRandomSleepTime(2, 3));
                                IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("textarea"));

                                foreach (IWebElement we in welements2)
                                {
                                    if (we.GetAttribute("data-testid") == "tweetTextarea_0")
                                    {
                                        we.SendKeys(GetRandomTweet());
                                        stage = 19;
                                        break;
                                    }
                                }
                            }
                        
                        if (stage == 19)
                            {
                                Thread.Sleep(GetRandomSleepTime(2, 3));

                                IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("div"));
                                foreach (IWebElement we in welements2)
                                {
                                    if (we.GetAttribute("role") == "button" && we.GetAttribute("data-testid") == "tweetButton")
                                    {
                                        we.Click();
                                        stage = 20;
                                        break;
                                    }
                                }
                            }
                        


                     
                        if (stage == 20)
                        {
                            Thread.Sleep(GetRandomSleepTime(3, 4));

                            lbl_cachecounter.Text = counter.ToString();
                            string output = JsonConvert.SerializeObject(new Kayit { counter = (counter + 1).ToString(), counter_formail = counter_formail.ToString() });
                            if (File.Exists(Application.StartupPath + "\\save.svx"))
                                File.Delete(Application.StartupPath + "\\save.svx");

                            File.WriteAllText(Application.StartupPath + "\\save.svx", output);

                            try
                            {
                                string getkullaniciadi = "error";
                                IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("span"));

                                foreach (IWebElement we in welements2)
                                {
                                    if (we.Text.Contains("@"))
                                    {
                                        getkullaniciadi = we.Text.Replace("@", "");
                                        break;
                                    }
                                    //if (we.GetAttribute("href").StartsWith("/"))
                                    //{
                                    //    getkullaniciadi = we.GetAttribute("href").Replace("/", "");
                                    //    break;
                                    //}
                                }
                                File.AppendAllText(Application.StartupPath + "\\acilan_uyelikler.txt", getkullaniciadi + ":" + randompass + ":" + cache_mails[counter].Split(':')[0] + "\r\n");
                                stage = 21;

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                        }

                        if (stage == 21)
                        {
                            Thread.Sleep(GetRandomSleepTime(2, 3));

                            WD.Navigate().GoToUrl("https://mobile.twitter.com/settings/profile");
                            WaitForPageLoad();
                            stage = 22;
                        }

                        if (chk_arkaplan.Checked && stage == 22)
                        {
                            if (stage == 22)
                            {
                                Thread.Sleep(GetRandomSleepTime(3, 4));

                                IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("input"));

                                foreach (IWebElement we in welements2)
                                {
                                    if (we.GetAttribute("accept") == "image/jpeg,image/png,image/webp")
                                    {
                                        we.Clear();
                                        we.SendKeys(GetRandomWallpaperFile());
                                        stage = 23;
                                        break;
                                    }
                                }
                            }

                            if (stage == 23)
                            {
                                Thread.Sleep(GetRandomSleepTime(2, 3));
                                if (GecmeButonuTikla())
                                    stage = 24;

                            }
                        }
                        else if(!chk_arkaplan.Checked && stage == 22)
                        {
                            stage = 24;
                        }
                       

                        if (stage == 24)
                        {
                            Thread.Sleep(GetRandomSleepTime(2, 3));
                            if (IsTestElementPresent(By.Name("description")))
                            {
                                HoverMouseOverGO(WD.FindElement(By.Name("description")));

                                stage = 25;
                            }
                        }

                        if (chk_lokasyon.Checked && stage == 25)
                        {
                            if (stage == 25)
                            {
                                Thread.Sleep(GetRandomSleepTime(2, 3));
                                if (IsTestElementPresent(By.Name("location")))
                                {
                                    HoverMouseOverinput(WD.FindElement(By.Name("location")), GetRandomKonum());

                                    stage = 26;
                                }
                            }
                        }
                        else if(!chk_lokasyon.Checked && stage == 25)
                        {
                            stage = 26;
                        }
                        

                        if (chk_website.Checked && stage == 26)
                        {
                            if (stage == 26)
                            {
                                Thread.Sleep(GetRandomSleepTime(2, 3));
                                if (IsTestElementPresent(By.Name("url")))
                                {
                                    HoverMouseOverinput(WD.FindElement(By.Name("url")), GetRandomSite());
                                    stage = 27;
                                }
                            }
                        }
                        else if (!chk_website.Checked && stage == 26)
                        {
                            stage = 27;
                        }
                       

                        if (stage == 27)
                        {
                            Thread.Sleep(GetRandomSleepTime(2, 3));

                            IReadOnlyCollection<IWebElement> welements2 = WD.FindElements(By.TagName("div"));
                            foreach (IWebElement we in welements2)
                            {
                                if (we.GetAttribute("role") == "button" && we.GetAttribute("data-testid") == "Profile_Save_Button")
                                {
                                    we.Click();
                                    stage = 28;
                                    break;
                                }
                            }
                        }

                        if (chk_takip_txt.Checked && stage == 28)
                        {
                            if (stage == 28)
                            {
                                Thread.Sleep(GetRandomSleepTime(2, 3));
                                foreach (string str in takip_edilecekler)
                                {
                                    WD.Navigate().GoToUrl("https://mobile.twitter.com/" + str);
                                    WaitForPageLoad();
                                    Thread.Sleep(GetRandomSleepTime(1, 3));
                                    if (IsElementDisplayed(By.XPath("//div[@role='button']//span[contains(text(),'Takip et') or contains(text(),'Follow')]")))
                                    {
                                        WD.FindElement(By.XPath("//div[@role='button']//span[contains(text(),'Takip et') or contains(text(),'Follow')]")).Click();
                                    }
                                    Thread.Sleep(GetRandomSleepTime(1, 3));

                                }
                                Thread.Sleep(GetRandomSleepTime(1, 2));
                                WD.Navigate().GoToUrl("https://mobile.twitter.com/home");
                                WaitForPageLoad();
                                Thread.Sleep(GetRandomSleepTime(2, 3));
                                stage = 29;
                            }
                        }
                        else if (!chk_takip_txt.Checked && stage == 28)
                        {
                            Thread.Sleep(GetRandomSleepTime(1, 2));
                            WD.Navigate().GoToUrl("https://mobile.twitter.com/home");
                            WaitForPageLoad();
                            Thread.Sleep(GetRandomSleepTime(2, 3));
                            stage = 29;
                        }
                        

                        randompass = "";
                        counter++;
                        WD.Close();
                        WD.Quit();
                        WD.Dispose();
                        stage = -1;
                        AndroidReset();
                        break;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.ToString());
                    MessageBox.Show(stage.ToString());
                }
            }

            if (stage == -1)
                ControlBrowser();
            
                    
        }

        private void OpenSubMails(string mail, string pw)
        {


            try
            {
                ChromeOptions CO = new ChromeOptions();
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                Random rnd = new Random();
                int randomum = rnd.Next(0, u_agents.Length - 1);

                CO.AddArgument("incognito");
                // CO.AddArgument("headless"); // chrome gizleme

                WD = new ChromeDriver(service, CO);

                WebDriverWait wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));

                WD.Navigate().GoToUrl("https://www.mail.com/int/");
                wdw.Until(ExpectedConditions.ElementToBeClickable(By.Id("login-button"))).Click();

                wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                wdw.Until(ExpectedConditions.ElementIsVisible(By.Id("login-email"))).SendKeys(mail);

                wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                wdw.Until(ExpectedConditions.ElementIsVisible(By.Id("login-password"))).SendKeys(pw);


                wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                wdw.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("login-submit"))).Click();

                WaitForPageLoad();

                //try
                //{
                //    wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                //    wdw.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(By.Id("thirdPartyFrame_splash_nav")));

                //    IReadOnlyCollection<IWebElement> txtMail = WD.FindElements(By.TagName("a"));
                //    foreach (IWebElement we in txtMail)
                //    {
                //        if (we.GetAttribute("href") == "#")
                //        {
                //            we.Click();
                //        }
                //    }

                //}
                //catch
                //{

                //}

                wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                wdw.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(By.Id("thirdPartyFrame_home")));


                wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                wdw.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("aliasaddresses"))).Click();

                WD.SwitchTo().DefaultContent();

                wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                wdw.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(By.Id("thirdPartyFrame_mail")));

                if (File.Exists(Application.StartupPath + "\\Mail_Cache.txt"))
                    File.Delete(Application.StartupPath + "\\Mail_Cache.txt");

                File.AppendAllText(Application.StartupPath + "\\Mail_Cache.txt", mail + ":" + pw + "\r\n");

                for (int i = 0; i < 9; i++)
                {
                    wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                    IWebElement button = wdw.Until(ExpectedConditions.ElementToBeClickable(By.Id("id6")));

                    IReadOnlyCollection<IWebElement> txtMail = WD.FindElements(By.TagName("input"));
                    foreach (IWebElement we in txtMail)
                    {
                        if (we.GetAttribute("placeholder") == "e.g. your-name")
                        {
                            we.Clear();
                            //Random rnd2 = new Random();
                            //int K_E = rnd2.Next(0, 100);

                            //Random rnd3 = new Random();
                            //int rndfork = rnd3.Next(0, k_isimler.Length - 1);

                            //Random rnd4 = new Random();
                            //int rndfore = rnd4.Next(0, e_isimler.Length - 1);

                            //Random rnd5 = new Random();
                            //int rnddogum = rnd5.Next(60, 99);

                            //if (K_E > 50)
                            //{
                            //    we.SendKeys(k_isimler[rndfork].ToLower().Replace(" ", "_") + "19" + rnddogum.ToString());
                            //}
                            //else
                            //{
                            //    we.SendKeys(e_isimler[rndfore].ToLower().Replace(" ", "_") + "19" + rnddogum.ToString());
                            //}

                            we.SendKeys(mail.Replace("@mail.com", i.ToString()));
                            File.AppendAllText(Application.StartupPath + "\\Mail_Cache.txt", mail.Replace("@mail.com", i.ToString()) + "@mail.com:" + pw + "\r\n");
                        }
                    }
                    //

                    button.Click();
                    //wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 5));
                    //if (wdw.Until(ExpectedConditions.ElementIsVisible(By.Id("id2a"))) == null)
                    //{
                    //    wdw = new WebDriverWait(WD, new TimeSpan(0, 0, 10));
                    //    wdw.Until(ExpectedConditions.ElementToBeClickable(By.Id("id32"))).Click();
                    //}

                    Thread.Sleep(5000);


                }

                WD.Close();
                WD.Quit();
                WD.Dispose();

                counter = 0;
                lbl_cachecounter.Text = counter.ToString();
                lbl_normalcounter.Text = counter_formail.ToString();
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("thirdPartyFrame_home"))
                {
                    if (WD.Url.ToString().Contains("AbuseHardLock"))
                    {
                        WD.Close();
                        WD.Quit();
                        WD.Dispose();

                        Thread.Sleep(GetRandomSleepTime(3,4));
                        counter_formail++;
                        OpenSubMails(mailandpw[counter_formail].Split(':')[0], mailandpw[counter_formail].Split(':')[1]);
                        return;
                    }
                    else
                    {
                        WD.Close();
                        WD.Quit();
                        WD.Dispose();

                        Thread.Sleep(GetRandomSleepTime(3,4));
                        OpenSubMails(mail, pw);
                        return;
                    }
                    

                }
                else
                MessageBox.Show(ex.ToString());
            }

        }

        private void AndroidReset()
        {
            UcakModuAc(mDevice);
            System.Threading.Thread.Sleep(2000);
            UcakModuKapat(mDevice);
            System.Threading.Thread.Sleep(3000);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFiles();
            //WD.Navigate().GoToUrl("https://twitter.com/i/flow/signup");
           

        }

        private void MaterialFlatButton1_Click(object sender, EventArgs e)
        {
            Thread mth = new Thread(() => ControlBrowser());
            mth.Start();
            thread_Pool.Add(mth);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            string output = JsonConvert.SerializeObject(new Kayit { counter = (counter + 1).ToString(), counter_formail = counter_formail.ToString() });
            if (File.Exists(Application.StartupPath + "\\save.svx"))
                File.Delete(Application.StartupPath + "\\save.svx");

            File.WriteAllText(Application.StartupPath + "\\save.svx", output);

            if (thread_Pool.Count > 0)
            {
                foreach (Thread th in thread_Pool)
                    th.Abort();
            }

            if (WD != null)
                WD.Quit();

            Process[] a = Process.GetProcessesByName("chromedriver");

            foreach(Process b in a)
            {
                b.Kill();
            }

            System.Threading.Thread.Sleep(GetRandomSleepTime(2,3));

            
            

        }

        private void MaterialFlatButton2_Click(object sender, EventArgs e)
        {
            OpenSubMails(cache_mails[counter_formail].Split(':')[0], cache_mails[counter_formail].Split(':')[1]);
        }

        private void MaterialFlatButton3_Click(object sender, EventArgs e)
        {
            ChromeOptions CO = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            Random rnd = new Random();
            int randomum = rnd.Next(0, u_agents.Length - 1);

            var proxy = new Proxy();

            proxy.SslProxy = "63.134.172.12:80";



            // CO.AddArgument("--user-agent=\"" + u_agents[randomum] + "\"");
            // CO.AddArgument("incognito");
            CO.Proxy = proxy;
            
            // CO.AddArgument("headless"); // chrome gizleme

            WD = new ChromeDriver(service, CO);
            WD.Navigate().GoToUrl("https://google.com");
        }

        private void MaterialFlatButton4_Click(object sender, EventArgs e)
        {
            AS = new AdbServer();
            AS.StartServer(Application.StartupPath + "\\adb.exe", false);

            if (AdbClient.Instance.GetDevices().Count == 0)
            {
                lbl_aygitdurum.Text = "Hiç bir cihaz bulunamadı";
                MessageBox.Show("Bağlı hiç bir cihaz bulunamadı!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
                

            mDevice = AdbClient.Instance.GetDevices().First();


            if (mDevice != null)
            {
                lbl_aygitdurum.Text = "Aygıt ile bağlantı kuruldu";
            }
            else
            {
                lbl_aygitdurum.Text = "Aygıt ile bağlantı kurulamadı";
            }
        }

        private void MaterialFlatButton5_Click(object sender, EventArgs e)
        {
            AndroidReset();
        }

        private void MaterialFlatButton6_Click(object sender, EventArgs e)
        {
            ChromeOptions CO = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            Random rnd = new Random();
            int randomum = rnd.Next(0, u_agents.Length - 1);

            CO.AddArgument("--user-agent=\"" + u_agents[randomum] + "\"");
            CO.AddArgument("--window-size=1300,1000");
            CO.AddArgument("incognito");
            // CO.AddArgument("headless"); // chrome gizleme

            WD = new ChromeDriver(service, CO);
            WD.Navigate().GoToUrl("https://twitter.com/i/flow/signup");

            WaitForPageLoad();

            Thread.Sleep(5000);

            if (IsTestElementPresent(By.XPath("//div[@dir='auto' and @role='button']")))
            {
                HoverMouseOverClick(WD.FindElement(By.XPath("//div[@dir='auto' and @role='button']")));
            }
        }

        private void MaterialFlatButton7_Click(object sender, EventArgs e)
        {
            GetRandomKonum();
        }
    }
}
