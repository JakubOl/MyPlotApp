﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dtos
{
	public class RoleDto
	{
        [Required]
		public string RoleName { get; set; }	
	}
}
