using System.Runtime.Serialization.Json;
using Tutorial3.Models;

public class EmpDeptSalgradeTests
{
    // 1. Simple WHERE filter
    // SQL: SELECT * FROM Emp WHERE Job = 'SALESMAN';
    [Fact]
    public void ShouldReturnAllSalesmen()
    {
        var emps = Database.GetEmps();

        List<Emp> result = new List<Emp>();
        
        foreach (Emp e in emps)
        {
            result.Add(e);
        }

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("SALESMAN", e.Job));
    }

    // 2. WHERE + OrderBy
    // SQL: SELECT * FROM Emp WHERE DeptNo = 30 ORDER BY Sal DESC;
    [Fact]
    public void ShouldReturnDept30EmpsOrderedBySalaryDesc()
    {
        var emps = Database.GetEmps();

        List<Emp> result = new List<Emp>();
        
        foreach (Emp e in emps)
        {
            result.Add(e);
        }

        Assert.Equal(2, result.Count);
        Assert.True(result[0].Sal >= result[1].Sal);
    }

    // 3. Subquery using LINQ (IN clause)
    // SQL: SELECT * FROM Emp WHERE DeptNo IN (SELECT DeptNo FROM Dept WHERE Loc = 'CHICAGO');
    [Fact]
    public void ShouldReturnEmployeesFromChicago()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        List<Emp> result = new List<Emp>(); 
        
        foreach (Emp e in emps)
        {
            result.Add(e);
        }

        Assert.All(result, e => Assert.Equal(30, e.DeptNo));
    }

    // 4. SELECT projection
    // SQL: SELECT EName, Sal FROM Emp;
    [Fact]
    public void ShouldSelectNamesAndSalaries()
    {
        var emps = Database.GetEmps();

        var result = new List<Emp>();

        foreach (Emp e in emps)
        {
            result.Add(e);
        }

         Assert.All(result, r =>
         {
             Assert.False(string.IsNullOrWhiteSpace(r.EName));
             Assert.True(r.Sal > 0);
         });
    }

    // 5. JOIN Emp to Dept
    // SQL: SELECT E.EName, D.DName FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo;
    [Fact]
    public void ShouldJoinEmployeesWithDepartments()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        var result = (
            from e in emps 
            join d in depts on e.DeptNo equals d.DeptNo
            select new { EName = e.EName, DName = d.DName }
            ).ToList();

        Assert.Contains(result, r => r.DName == "SALES" && r.EName == "ALLEN");
    }

    // 6. Group by DeptNo
    // SQL: SELECT DeptNo, COUNT(*) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCountEmployeesPerDepartment()
    {
        var emps = Database.GetEmps();

        var result = (
            from e in emps
            group e by e.DeptNo into gr
            select new
            {
                DeptNo = gr.Key,
                Count = gr.Count()
            }); 
        
        Assert.Contains(result, g => g.DeptNo == 30 && g.Count == 2);
    }

    // 7. SelectMany (simulate flattening)
    // SQL: SELECT EName, Comm FROM Emp WHERE Comm IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithCommission()
    {
        var emps = Database.GetEmps();

        var result = (
            from e in emps
            where e.Comm != null
            select new
            {
                EName = e.EName,
                Comm = e.Comm
            }).ToList(); 
        
        Assert.All(result, r => Assert.NotNull(r.Comm));
    }

    // 8. Join with Salgrade
    // SQL: SELECT E.EName, S.Grade FROM Emp E JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldMatchEmployeeToSalaryGrade()
    {
        var emps = Database.GetEmps();
        var grades = Database.GetSalgrades();
        
        /*var result = (
            from e in emps
            join g in grades
            where e.Sal >= g.Losal && e.Sal <= g.Hisal
            select new
        {
            EName = e.EName,
            Grade = g.Grade
        }).ToList();*/
        
        var result = (
            from e in emps
            from g in grades
            where e.Sal >= g.Losal && e.Sal <= g.Hisal
            select new
            {
                EName = e.EName,
                Grade = g.Grade
            }).ToList();
        
        //
        // Assert.Contains(result, r => r.EName == "ALLEN" && r.Grade == 3);
    }

    // 9. Aggregation (AVG)
    // SQL: SELECT DeptNo, AVG(Sal) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCalculateAverageSalaryPerDept()
    {
        var emps = Database.GetEmps();

        var result = (
            from e in emps
            group e by e.DeptNo into gr
            select new
        {
            DeptNo = gr.Key,
            AvgSal = gr.Average(e => e.Sal)
        }).ToList(); 
        
        Assert.Contains(result, r => r.DeptNo == 30 && r.AvgSal > 1000);
    }

    // 10. Complex filter with subquery and join
    // SQL: SELECT E.EName FROM Emp E WHERE E.Sal > (SELECT AVG(Sal) FROM Emp WHERE DeptNo = E.DeptNo);
    [Fact]
    public void ShouldReturnEmployeesEarningMoreThanDeptAverage()
    {
        var emps = Database.GetEmps();

        var avgsal = (from e in emps select e.Sal).Average();
        
        var result = (
            from e in emps
            where e.Sal < avgsal
            select new
        {
            DeptNo = e.DeptNo,
            Sal = e.Sal
        }).ToList();
        // todo
        //Assert.Contains("ALLEN", result);
    }
}
