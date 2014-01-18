﻿using System;
using NUnit.Framework;

namespace XmlDiff.Tests
{
	[TestFixture]
	public class DiffValueTests
	{
		private readonly string Raw = "theValue";

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_ShouldNotAllowRaw_ToBeNull(string raw)
		{
			new DiffValue(DiffAction.Added, raw);
		}

		[Test]
		[TestCase(DiffAction.Added)]
		[TestCase(DiffAction.Removed)]
		public void IsChangedProperty_ShouldAlwaysBeTrue(DiffAction action)
		{
			var diffVal = new DiffValue(action, Raw);
			Assert.IsTrue(diffVal.IsChanged);
		}

		[Test]
		public void ToString_ReturnStringRepresentation()
		{
			var diffVal = new DiffValue(DiffAction.Added, Raw);
			string result = diffVal.ToString();
			string expected = "+ Value: \"theValue\"\r\n";
			Assert.AreEqual(expected, result);
		}
	}
}
