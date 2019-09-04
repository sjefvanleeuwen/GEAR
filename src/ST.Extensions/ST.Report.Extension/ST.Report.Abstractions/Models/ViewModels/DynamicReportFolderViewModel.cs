﻿using System;

namespace ST.Report.Abstractions.Models.ViewModels
{
    public class DynamicReportFolderViewModel
    {
        public DynamicReportFolderViewModel(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
