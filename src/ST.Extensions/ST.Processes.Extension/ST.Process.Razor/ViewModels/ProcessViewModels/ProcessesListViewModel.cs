
using ST.Procesess.Models;

namespace ST.Process.Razor.ViewModels.ProcessViewModels
{
	public class ProcessesListViewModel: STProcessSchema
	{
		public string AuthorName { get; set; }
		public string ModifiedByName { get; set; }
		public string CreatedString { get; set; }
		public string ChangedString { get; set; }
	}
}