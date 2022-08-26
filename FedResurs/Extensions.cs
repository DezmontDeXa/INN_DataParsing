using RestSharp;

namespace FedResurs
{
    public static class Extensions
    {
        public static RestRequest AddForm(this RestRequest request, string viewState, string viewStateGenerator, string previewPage, string inn)
        {
            return request
                .AddParameter("__EVENTTARGET", "", true)
                .AddParameter("__EVENTARGUMENT", "", true)
                .AddParameter("__VIEWSTATE", viewState, true)
                .AddParameter("__VIEWSTATEGENERATOR", viewStateGenerator, true)
                .AddParameter("__PREVIOUSPAGE", previewPage, true)
                .AddParameter("ctl00$PrivateOffice1$tbLogin", "", true)
                .AddParameter("ctl00$PrivateOffice1$tbPassword", "", true)
                .AddParameter("ctl00$PrivateOffice1$cbRememberMe", "on", true)
                .AddParameter("ctl00$PrivateOffice1$tbEmailForPassword", "", true)
                .AddParameter("ctl00_PrivateOffice1_RadToolTip1_ClientState", "", true)
                .AddParameter("ctl00$DebtorSearch1$inputDebtor", "поиск", true)
                .AddParameter("ctl00$cphBody$rblDebtorType", "Persons", true)
                .AddParameter("ctl00$cphBody$tbOrgName", "", true)
                .AddParameter("ctl00$cphBody$tbOrgAddress", "", true)
                .AddParameter("ctl00$cphBody$ucOrgRegionList$ddlBoundList", "", true)
                .AddParameter("ctl00$cphBody$ucOrgCategoryList$ddlBoundList", "", true)
                .AddParameter("ctl00$cphBody$OrganizationCode1$CodeTextBox", "", true)
                .AddParameter("ctl00$cphBody$tbPrsLastName", "", true)
                .AddParameter("ctl00$cphBody$tbPrsFirstName", "", true)
                .AddParameter("ctl00$cphBody$tbPrsMiddleName", "", true)
                .AddParameter("ctl00$cphBody$tbPrsAddress", "", true)
                .AddParameter("ctl00$cphBody$ucPrsRegionList$ddlBoundList", "", true)
                .AddParameter("ctl00$cphBody$ucPrsCategoryList$ddlBoundList", "", true)
                .AddParameter("ctl00$cphBody$PersonCode1$CodeTextBox", inn, true)
                .AddParameter("ctl00$cphBody$btnSearch.x", "18", true)
                .AddParameter("ctl00$cphBody$btnSearch.y", "2", true);
        }
    }
}