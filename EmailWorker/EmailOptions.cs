using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailWorker;

public class EmailOptions
{
    [Required]
    public required string From { get; set; }
}
