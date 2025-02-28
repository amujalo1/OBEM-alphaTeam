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

        [TestMethod]
        public void TestLoginButton_Click_InvalidCredentials_ShouldShowErrorMessage()
        {
            // Arrange
            _loginWindow.SetUsernameText("admin");
            _loginWindow.SetPassword("wrongpassword");

            // Act
            _loginWindow.LoginButton_Click(null, new RoutedEventArgs());

            // Assert: 
          
            Assert.AreEqual("Invalid username or password. Please try again.", _loginWindow.ErrorMessage);
        }

        [TestMethod]
        public void TestLoginButton_Click_EmptyUsername_ShouldShowErrorMessage()
        {
            // Arrange
            _loginWindow.SetUsernameText(""); // Empty username
            _loginWindow.SetPassword("1234");

            // Act
            _loginWindow.LoginButton_Click(null, new RoutedEventArgs());

            // Assert: 
            Assert.AreEqual("Please fill in both username and password.", _loginWindow.ErrorMessage);
        }

        [TestMethod]
        public void TestLoginButton_Click_EmptyPassword_ShouldShowErrorMessage()
        {
            // Arrange
            _loginWindow.SetUsernameText("admin");
            _loginWindow.SetPassword(""); // Empty password

            // Act
            _loginWindow.LoginButton_Click(null, new RoutedEventArgs());

            // Assert:
            Assert.AreEqual("Please fill in both username and password.", _loginWindow.ErrorMessage);
        }

        [TestMethod]
        public void TestLoginButton_Click_EmptyUsernameAndPassword_ShouldShowErrorMessage()
        {
            // Arrange
            _loginWindow.SetUsernameText(""); 
            _loginWindow.SetPassword(""); 

            // Act
            _loginWindow.LoginButton_Click(null, new RoutedEventArgs());

            // Assert:
            Assert.AreEqual("Please fill in both username and password.", _loginWindow.ErrorMessage);
        }

        [TestMethod]
        public void TestLoginButton_Click_CancelledLogin_ShouldNotCloseWindow()
        {
            // Arrange
            _loginWindow.SetUsernameText("admin");
            _loginWindow.SetPassword("wrongpassword");

            // Act
            _loginWindow.LoginButton_Click(null, new RoutedEventArgs());

            // Assert
            Assert.IsTrue(_loginWindow.IsLoaded); 
        }

    }
}