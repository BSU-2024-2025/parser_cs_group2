using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TestCodeExecuter
{
    [TestFixture]
    public class CodeExecutorTests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
        }

        private void ExecuteTest(string title, string code, string expectedResult)
        {
            driver.Navigate().GoToUrl("https://localhost:7021/");

            driver.FindElement(By.Id("codeTitle")).Clear();
            driver.FindElement(By.Id("codeSnippet")).Clear();

            driver.FindElement(By.Id("codeTitle")).SendKeys(title);
            driver.FindElement(By.Id("codeSnippet")).SendKeys(code);

            driver.FindElement(By.XPath("//button[text()='Save']")).Click();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("codeList")));

            var runButton = wait.Until(d =>
                d.FindElement(By.XPath($"//strong[text()='{title}']/following-sibling::button[text()='Run']")));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", runButton);

            wait.Until(d => {
                var element = d.FindElement(By.XPath("//button[text()='Run']"));
                return element.Displayed && element.Enabled ? element : null;
            });

            runButton.Click();

            var result = wait.Until(d => d.FindElement(By.Id("result"))).Text;
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        
        [Test] public void Test1() { ExecuteTest("test1", "abc", "Error: Undefined variable: abc"); }
        [Test] public void Test2() { ExecuteTest("test2", "1 + 1", "2"); }
        [Test] public void Test3() { ExecuteTest("test3", "88 - 9", "79"); }
        [Test] public void Test4() { ExecuteTest("test4", "6 * 7", "42"); }
        [Test] public void Test5() { ExecuteTest("test5", "120 / 4", "30"); }
        [Test] public void Test6() { ExecuteTest("test6", "(1 + 2) * 3", "9"); }
        [Test] public void Test7() { ExecuteTest("test7", "5 + 3 * 2", "11"); }
        [Test] public void Test8() { ExecuteTest("test8", "10 - (3 + 2)", "5"); }
        [Test] public void Test9() { ExecuteTest("test9", "2 * (3 + 5)", "16"); }
        [Test] public void Test10() { ExecuteTest("test10", "3 + 4 * 2 - 1", "10"); }
        [Test] public void Test11() { ExecuteTest("test11", "- (2 + (4 / 2)) + 3", "-1"); }
        [Test] public void Test12() { ExecuteTest("test12", "a = 4 * 13", "a=52"); }
        [Test] public void Test13() { ExecuteTest("test13", "b = 8 * (6 - 1)", "b=40"); }
        [Test] public void Test14() { ExecuteTest("test14", "c = -1 - 4", "c=-5"); }

        [TearDown]
        public void Cleanup()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}