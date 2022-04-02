using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolutionSecrets.Core.Encryption;
using SolutionSecrets.Core.Repository;


namespace SolutionSecrets2019
{
	public static class Services
	{

		private static ICipher _cipher;
		public static ICipher Cipher => _cipher ?? (_cipher = new Cipher());


		private static IRepository _repository;
		public static IRepository Repository => _repository ?? (_repository = new GistRepository());

	}
}
