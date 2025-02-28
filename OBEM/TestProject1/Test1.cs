using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq; // You need Moq to mock the ApiService
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using OBEM;
using OBEM.Services;
using System.Windows.Controls;
using System;
using System.Diagnostics;

namespace OBEM_tests
{
    [TestClass]
    public class MainWindowTest
    {
        private MainWindow _mainWindow;
        private Mock<ApiService> _mockApiService;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockApiService = new Mock<ApiService>();

            Thread staThread = new Thread(() =>
            {
                Application app = new Application();
                _mainWindow = new MainWindow();
                app.Run(_mainWindow); 
            });
            staThread.SetApartmentState(ApartmentState.STA); 
            staThread.Start();
            staThread.Join(); 
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _mainWindow?.Close(); 
        }

        [TestMethod]
        public async Task Test_LoadHighConsumptionData_Success()
        {
            try
            {
                _mockApiService.Setup(service => service.GetAllDevicesAsync()).ReturnsAsync("[API JSON data here]");

                await _mainWindow.Dispatcher.InvokeAsync(async () =>
                {
                    await _mainWindow.LoadHighConsumptionData();
                });

                Assert.IsNotNull(_mainWindow.dgHighConsumption.ItemsSource); 
  is             }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test failed: {ex.Message}");
                throw; 
            }
        }

        // Test for loading anomalies data
        [TestMethod]
        public async Task Test_LoadAnomaliesData_Success()
        {
            try
            {
                _mockApiService.Setup(service => service.GetTrendingInfo()).ReturnsAsync("[API JSON data here]");

                await _mainWindow.Dispatcher.InvokeAsync(async () =>
                {
                    await _mainWindow.LoadAnomaliesData(); 
                });

                Assert.IsNotNull(_mainWindow.dgAnomalies.ItemsSource);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test failed: {ex.Message}");
                throw; 
            }
        }

        [TestMethod]
        public void Test_OpenNotificationsPage_ShouldNavigate()
        {
            try
            {
                _mainWindow.Dispatcher.Invoke(() =>
                {
                    var button = _mainWindow.FindName("NotificationsButton") as Button;
                    button?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); 
                });

                Assert.IsInstanceOfType(_mainWindow.MainFrame.Content, typeof(Notifikacije)); // Assert navigation to Notifikacije
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test failed: {ex.Message}");
                throw; 
            }
        }

        [TestMethod]
        public void Test_LogOut_ShouldCloseWindowAndShowLoginWindow()
        {
            try
            {
                _mainWindow.Dispatcher.Invoke(() =>
                {
                    var logOutButton = _mainWindow.FindName("LogOutButton") as Button;
                    logOutButton?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));   
                });

                Assert.IsTrue(_mainWindow == null || !Application.Current.Windows.Contains(_mainWindow)); // Assert MainWindow is closed
                Assert.IsInstanceOfType(Application.Current.Windows[0], typeof(LoginWindow)); // Assert LoginWindow is displayed
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test failed: {ex.Message}");
                throw; 
            }
        }

        [TestMethod]
        public async Task Test_Timer_Tick_ShouldUpdateData()
        {
            try
            {
                _mockApiService.Setup(service => service.GetAllDevicesAsync()).ReturnsAsync("[API JSON data here]");
                _mockApiService.Setup(service => service.GetTrendingInfo()).ReturnsAsync("[API JSON data here]");

                await _mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    _mainWindow.StartThreadTimer(); 
                });

                await _mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    _mainWindow.Timer_Tick(this, new EventArgs()); 
                });

                Assert.IsNotNull(_mainWindow.dgHighConsumption.ItemsSource);
                Assert.IsNotNull(_mainWindow.dgAnomalies.ItemsSource); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test failed: {ex.Message}");
                throw; 
            }
        }
    }
}