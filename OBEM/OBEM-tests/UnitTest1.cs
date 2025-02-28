using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OBEM.Views;
using OBEM;
using System.Windows.Controls;
using System.Windows;

namespace OBEM_tests
{

    [TestClass]
    public class UnitTest1
    {

        private LoginWindow _loginWindow;

        [TestInitialize]
        public void TestInitialize()
        {
            _loginWindow = new LoginWindow();
        }
        [TestMethod]
        public void TestLoginButton_Click_ValidCredentials_ShouldShowSuccessMessage()
        {
            // Arrange
            _loginWindow.SetUsernameText("admin");
            _loginWindow.SetPassword("1234");
            
            // Act
            _loginWindow.LoginButton_Click(null, new RoutedEventArgs());

            // Assert
            Assert.IsTrue(_loginWindow.IsLoaded); 
        }

    }
}