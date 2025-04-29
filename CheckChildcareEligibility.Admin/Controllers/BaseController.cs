using CheckChildcareEligibility.Admin.Domain.DfeSignIn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Controllers;

[Authorize]
public class BaseController : Controller
{
    protected DfeClaims? _Claims;
}