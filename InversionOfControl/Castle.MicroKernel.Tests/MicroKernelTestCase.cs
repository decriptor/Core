// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using Castle.Core;

namespace Castle.MicroKernel.Tests
{
	using System;
	using System.Collections;
	using Castle.MicroKernel.Tests.ClassComponents;
	using NUnit.Framework;

	[TestFixture]
	public class MicroKernelTestCase
	{
		private IKernel kernel;

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[Test]
		public void IOC_50_AddTwoComponentWithSameService_RequestFirstByKey_RemoveFirst_RequestByService_ShouldReturnSecond()
		{
			kernel.AddComponent("key", typeof(ICustomer), typeof(CustomerImpl));
			kernel.AddComponent("key2", typeof(ICustomer), typeof(CustomerImpl));
			object result = kernel["key"];
			Assert.IsNotNull(result);

			kernel.RemoveComponent("key");

			result = kernel[typeof(ICustomer)];
			Assert.IsNotNull(result);
		}

		[Test]
		public void AddClassComponentWithInterface()
		{
			kernel.AddComponent("key", typeof(CustomerImpl));
			Assert.IsTrue(kernel.HasComponent("key"));
		}

		[Test]
		public void AddClassComponentWithNoInterface()
		{
			kernel.AddComponent("key", typeof(DefaultCustomer));
			Assert.IsTrue(kernel.HasComponent("key"));
		}

		[Test]
		public void AddComponentInstance()
		{
			CustomerImpl customer = new CustomerImpl();

			kernel.AddComponentInstance("key", typeof(ICustomer), customer);
			Assert.IsTrue(kernel.HasComponent("key"));

			CustomerImpl customer2 = kernel["key"] as CustomerImpl;
			Assert.AreSame(customer, customer2);

			customer2 = kernel[typeof(ICustomer)] as CustomerImpl;
			Assert.AreSame(customer, customer2);
		}

		[Test]
		public void AddComponentInstance_ByService()
		{
			CustomerImpl customer = new CustomerImpl();

			kernel.AddComponentInstance <ICustomer>(customer);
			Assert.AreSame(kernel[typeof(ICustomer)],customer);
		}

		[Test]
		public void AddComponentInstance2()
		{
			CustomerImpl customer = new CustomerImpl();

			kernel.AddComponentInstance("key", customer);
			Assert.IsTrue(kernel.HasComponent("key"));

			CustomerImpl customer2 = kernel["key"] as CustomerImpl;
			Assert.AreSame(customer, customer2);

			customer2 = kernel[typeof(CustomerImpl)] as CustomerImpl;
			Assert.AreSame(customer, customer2);
		}

		[Test]
		public void AddCommonComponent()
		{
			kernel.AddComponent("key", typeof(ICustomer), typeof(CustomerImpl));
			Assert.IsTrue(kernel.HasComponent("key"));
		}

		[Test]
		public void HandlerForClassComponent()
		{
			kernel.AddComponent("key", typeof(CustomerImpl));
			IHandler handler = kernel.GetHandler("key");
			Assert.IsNotNull(handler);
		}

		[Test]
		public void HandlerForClassWithNoInterface()
		{
			kernel.AddComponent("key", typeof(DefaultCustomer));
			IHandler handler = kernel.GetHandler("key");
			Assert.IsNotNull(handler);
		}

		[Test]
		[ExpectedException(typeof(ComponentRegistrationException))]
		public void KeyCollision()
		{
			kernel.AddComponent("key", typeof(CustomerImpl));
			kernel.AddComponent("key", typeof(CustomerImpl));
		}

		[Test]
		[ExpectedException(typeof(ComponentNotFoundException))]
		public void UnregisteredComponentByKey()
		{
			kernel.AddComponent("key1", typeof(CustomerImpl));
			object component = kernel["key2"];
		}

		[Test]
		[ExpectedException(typeof(ComponentNotFoundException))]
		public void UnregisteredComponentByService()
		{
			kernel.AddComponent("key1", typeof(CustomerImpl));
			object component = kernel[typeof(IDisposable)];
		}

		[Test]
		public void AddClassThatHasTwoParametersOfSameTypeAndNoOverloads()
		{
			kernel.AddComponent("test", typeof(ClassWithTwoParametersWithSameType));
			kernel.AddComponent("test2", typeof(ICommon), typeof(CommonImpl1));
			object resolved = kernel.Resolve(typeof(ClassWithTwoParametersWithSameType), new Hashtable());
			Assert.IsNotNull(resolved);
		}

		[Test]
		public void ResolveAll()
		{
			kernel.AddComponent("test", typeof(ICommon), typeof(CommonImpl2));
			kernel.AddComponent("test2", typeof(ICommon), typeof(CommonImpl1));
			ICommon[] services = kernel.ResolveAll<ICommon>();
			Assert.AreEqual(2, services.Length);
		}

		[Test]
		public void ResolveAllWaitingOnDependencies()
		{
			kernel.AddComponent("test", typeof(ICommon), typeof(CommonImplWithDependancy));
			ICommon[] services = kernel.ResolveAll<ICommon>();
			Assert.AreEqual(0, services.Length);
		}

		[Test]
		public void ResolveViaGenerics()
		{
			kernel.AddComponent("cust", typeof(ICustomer), typeof(CustomerImpl));
			kernel.AddComponent("cust2", typeof(ICustomer), typeof(CustomerImpl2));
			ICustomer customer = kernel.Resolve<ICustomer>("cust");

			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("name", "customer2Name");
			dictionary.Add("address", "customer2Address");
			dictionary.Add("age", 18);
			ICustomer customer2 = kernel.Resolve<ICustomer>("cust2", dictionary);

			Assert.AreEqual(customer.GetType(), typeof(CustomerImpl));
			Assert.AreEqual(customer2.GetType(), typeof(CustomerImpl2));
		}

		[Test,ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage = "Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.\r\n As such, it should not be registered as implementation of Castle.MicroKernel.Tests.ClassComponents.ICommon service")]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_Simple_Signature()
		{
			kernel.AddComponent("abstract", typeof(ICommon), typeof(BaseCommonComponent));
		}

		[Test,ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage = "Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.\r\n As such, it should not be registered as implementation of Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent service")]
		public void ShouldNotRegisterAbstractClass_With_Simple_Signature()
		{
			kernel.AddComponent("abstract", typeof(BaseCommonComponent));
		}

		[Test,ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage = "Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.\r\n As such, it should not be registered as implementation of Castle.MicroKernel.Tests.ClassComponents.ICommon service")]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_LifestyleType_Signature()
		{
			kernel.AddComponent("abstract", typeof(ICommon), typeof(BaseCommonComponent), LifestyleType.Pooled);
		}

		[Test,ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage = "Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.\r\n As such, it should not be registered as implementation of Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent service")]
		public void ShouldNotRegisterAbstractClass_With_LifestyleType_Signature()
		{
			kernel.AddComponent("abstract", typeof(BaseCommonComponent), LifestyleType.Pooled);
		}


		[Test,ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage = "Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.\r\n As such, it should not be registered as implementation of Castle.MicroKernel.Tests.ClassComponents.ICommon service")]
		public void ShouldNotRegisterAbstractClassAsComponentImplementation_With_LifestyleType_And_Override_Signature()
		{
			kernel.AddComponent("abstract", typeof(ICommon), typeof(BaseCommonComponent), LifestyleType.Pooled, true);
		}

		[Test]
		[ExpectedException(typeof(ComponentRegistrationException),
			ExpectedMessage = "Type Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent is abstract.\r\n As such, it should not be registered as implementation of Castle.MicroKernel.Tests.ClassComponents.BaseCommonComponent service")]
		public void ShouldNotRegisterAbstractClass_With_LifestyleType_And_Override_Signature()
		{
			kernel.AddComponent("abstract", typeof(BaseCommonComponent), LifestyleType.Pooled, true);
		}

	}
}
