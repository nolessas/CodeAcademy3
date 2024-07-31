using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

[TestClass]
public class FileOperationsTests
{
    [TestMethod]
    public void SaveAndLoadAccounts_ShouldPreserveAccountData()
    {
        // Arrange
        var accounts = new List<BankAccount>
        {
            new BankAccount { Id = "1234567890123456", Balance = 1000 },
            new BankAccount { Id = "9876543210987654", Balance = 2000 }
        };

        // Act
        FileOperations.SaveAccounts(accounts);
        var loadedAccounts = FileOperations.LoadAccounts();

        // Assert
        Assert.AreEqual(accounts.Count, loadedAccounts.Count);
        for (int i = 0; i < accounts.Count; i++)
        {
            Assert.AreEqual(accounts[i].Id, loadedAccounts[i].Id);
            Assert.AreEqual(accounts[i].Balance, loadedAccounts[i].Balance);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(FileNotFoundException))]
    public void LoadAccounts_NonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        if (File.Exists("accounts.txt"))
        {
            File.Delete("accounts.txt");
        }

        // Act
        FileOperations.LoadAccounts();
    }
}