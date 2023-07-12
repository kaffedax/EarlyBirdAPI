using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskAPI.Class;

namespace TaskAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PackageController : ControllerBase
    {
        private static readonly List<Package> _packages = new()
        {
            //KolliId där måtten ej överskrids och ett korrekt KolliId där måtten överskrids
            new Package
            {
                KolliId = "999123456789012345", //Korrekt
                Weight = 10000,
                Length = 30,
                Height = 40,
                Width = 50,
                IsValid = true
            },
            new Package
            {
                KolliId = "999987654321098765", //Överskrids
                Weight = 20001,
                Length = 61,
                Height = 61,
                Width = 61,
                IsValid = false
            }
        };
        private readonly PackageLimits _packageLimits;

        public PackageController(PackageLimits packageLimits)
        {
            _packageLimits = packageLimits;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string?>> GetAllPackages()
        {
            try
            {
                return Ok(_packages.Select(p => p.KolliId));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{kolliid}")]
        public ActionResult<Package> GetPackageDetails(string kolliid)
        {
            try
            {
                if (!IsValidKolliId(kolliid))
                {
                    return BadRequest("Ogiltigt KolliId");
                }

                var package = _packages.FirstOrDefault(p => p.KolliId == kolliid);

                if (package == null)
                {
                    return NotFound("Paketet hittades inte");
                }

                return package;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<string> CreatePackage(Package package)
        {
            try
            {
                if (package == null || !ModelState.IsValid)
                {
                    return BadRequest("Ogiltig inmatning");
                }

                var packageDetails = new Package
                {
                    KolliId = GenerateKolliId(),
                    Weight = package.Weight,
                    Length = package.Length,
                    Height = package.Height,
                    Width = package.Width,
                    IsValid = IsPackageWithinLimits(package)
                };

                _packages.Add(packageDetails);

                return Created($"/package/{packageDetails.KolliId}", packageDetails.KolliId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        private static bool IsValidKolliId(string kolliid)
        {
            return !string.IsNullOrEmpty(kolliid) && kolliid.Length == 18 && kolliid.All(char.IsDigit);
        }

        private bool IsPackageWithinLimits(Package package)
        {
            Console.WriteLine($"Package Limits: Weight={_packageLimits.Weight}, Length={_packageLimits.Length}, Height={_packageLimits.Height}, Width={_packageLimits.Width}");

            return package.Weight > 0 && package.Length > 0 && package.Height > 0 && package.Width > 0
                && package.Weight <= _packageLimits.Weight && package.Length <= _packageLimits.Length
                && package.Height <= _packageLimits.Height && package.Width <= _packageLimits.Width;
        }

        private static string GenerateKolliId()
        {
            var random = new Random();
            var kolliId = "999";

            while (kolliId.Length < 18)
            {
                kolliId += random.Next(10).ToString();
            }

            return kolliId;
        }

    }
}