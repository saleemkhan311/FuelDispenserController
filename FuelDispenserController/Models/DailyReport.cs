using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace FuelDispenserController.Models;

public class DailyReport
{
    [PrimaryKey]
    public string Token
    {
        get; set;
    }

    public string OperatorName
    {
        get; set;
    }
    public decimal Quantity
    {
        get; set;
    }
    public decimal Rate
    {
        get; set;
    }
    public decimal TotalAmount
    {
        get; set;
    }
    public DateTime Date_Time
    {
        get; set;
    }

    public string User
    {
        get; set;
    }
}