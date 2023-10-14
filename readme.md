# lenh tao cac model
dotnet ef dbcontext scaffold -o Models/Entity -f -d "Data Source=localhost;Initial Catalog=DSP_API;User Id=sa;Password=123456;Trust Server Certificate=true" "Microsoft.EntityFrameworkCore.SqlServer"
# add view: 
builder.Services.AddControllersWithViews();

# base: 
co cau hinh 1 so thu de khi action duoc thuc hien se in ra man hinh

# cau hinh default route co cac api
cau hinh san route o 1 lop roi ke thua

# An action
[ApiExplorerSettings(IgnoreApi = true)] : an api nhung van co the truy cap qua link ()
private : an 100%

<!--  start use jwt -->