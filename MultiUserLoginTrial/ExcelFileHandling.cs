using MultiLogin.Models;
using ClosedXML.Excel;
namespace MultiUserLoginTrial
{
    public class ExcelFileHandling
    {
        //This Method will Create an Excel Sheet and Store it in the Memory Stream Object
        //And return thar Memory Stream Object
 
        public MemoryStream CreateExcelFileOfAdmins(List<Admin> admins)
            {
                //Create an Instance of Workbook, i.e., Creates a new Excel workbook
                var workbook = new XLWorkbook();
                //Add a Worksheets with the workbook
                //Worksheets name is Employees
                IXLWorksheet worksheet = workbook.Worksheets.Add("Admins");
                //Create the Cell
                //First Row is going to be Header Row
                worksheet.Cell(1, 1).Value = "ID"; //First Row and First Column
                worksheet.Cell(1, 2).Value = "Organization Name"; //First Row and Second Column
                worksheet.Cell(1, 3).Value = "Admin Name"; //First Row and Third Column
                worksheet.Cell(1, 4).Value = "Admin Email"; //First Row and Fourth Column
                worksheet.Cell(1, 5).Value = "Admin City"; //First Row and Fifth Column
                worksheet.Cell(1, 6).Value = "Is Active"; //First Row and Sixth Column
                                                                //Data is going to stored from Row 2
                int row = 2;
                //Loop Through Each Employees and Populate the worksheet
                //For Each Employee increase row by 1
                foreach (var admin in admins)
                {
                worksheet.Cell(row, 1).Value = admin.AId;
                    worksheet.Cell(row, 2).Value = admin.OrganizationName;
                    worksheet.Cell(row, 3).Value = admin.AdminName;
                    worksheet.Cell(row, 4).Value = admin.AdminEmail;
                worksheet.Cell(row, 5).Value = admin.AdminCity;
                    worksheet.Cell(row, 6).Value = admin.IsActive;
                    row++; //Increasing the Data Row by 1
                }
                //Create an Memory Stream Object
                var stream = new MemoryStream();
                //Saves the current workbook to the Memory Stream Object.
                workbook.SaveAs(stream);
                //The Position property gets or sets the current position within the stream.
                //This is the next position a read, write, or seek operation will occur from.
                stream.Position = 0;
                return stream;
            }
        
    //---------------------------------------------------------------------------------------------------------------------
    //For Excel of Users
    public MemoryStream CreateExcelFileOfUsers(List<Users> users)
    {
        //Create an Instance of Workbook, i.e., Creates a new Excel workbook
        var workbook = new XLWorkbook();
        //Add a Worksheets with the workbook
        //Worksheets name is Employees
        IXLWorksheet worksheet = workbook.Worksheets.Add("Users");
        //Create the Cell
        //First Row is going to be Header Row
        worksheet.Cell(1, 1).Value = "ID"; //First Row and First Column
        worksheet.Cell(1, 2).Value = "User Designation"; //First Row and Second Column
        worksheet.Cell(1, 3).Value = "Name"; //First Row and Third Column
        worksheet.Cell(1, 4).Value = "Email"; //First Row and Fourth Column
        worksheet.Cell(1, 5).Value = "City"; //First Row and Fifth Column
        worksheet.Cell(1, 6).Value = "Is Active"; //First Row and Sixth Column
           // worksheet.Cell(1, 7).Value = "Urls Json";                                          //Data is going to stored from Row 2
            int row = 2;
        //Loop Through Each Employees and Populate the worksheet
        //For Each Employee increase row by 1
        foreach (var user in users)
        {
                worksheet.Cell(row, 1).Value = user.UId;
            worksheet.Cell(row, 2).Value = user.UserDesignation;
            worksheet.Cell(row, 3).Value = user.UserName;
            worksheet.Cell(row, 4).Value = user.UserEmail;
            worksheet.Cell(row, 5).Value = user.UserCity;
            worksheet.Cell(row, 6).Value = user.IsActive;
               // worksheet.Cell(row, 7).Value = user.UrlsJson;

                row++; //Increasing the Data Row by 1
        }
        //Create an Memory Stream Object
        var stream = new MemoryStream();
        //Saves the current workbook to the Memory Stream Object.
        workbook.SaveAs(stream);
        //The Position property gets or sets the current position within the stream.
        //This is the next position a read, write, or seek operation will occur from.
        stream.Position = 0;
        return stream;
 
    }
}

}