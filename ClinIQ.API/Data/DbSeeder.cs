using ClinIQ.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinIQ.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // ── 1. Admin account ──────────────────────────────────────────
            if (!await db.Users.AnyAsync(u => u.Role == Roles.Admin))
            {
                db.Users.Add(new User
                {
                    FullName     = "Admin ClinIQ",
                    Email        = "admin@cliniq.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456", workFactor: 12),
                    Role         = Roles.Admin,
                    IsActive     = true,
                    CreatedAt    = DateTime.UtcNow,
                });
                await db.SaveChangesAsync();
                Console.WriteLine("✅ Admin account created  →  admin@cliniq.com / Admin@123456");
            }

            // ── 2. Doctors ────────────────────────────────────────────────
            if (await db.Doctors.AnyAsync()) return; // already seeded

            var seedDoctors = new[]
            {
                new SeedDoc("Dr. Sarah Hassan",    "د. سارة حسن",
                    "Cardiology",   "أمراض القلب",
                    "Board-certified cardiologist with 15+ years treating complex heart conditions. Fellow of the Egyptian Cardiology Society.",
                    "طبيبة قلب معتمدة بخبرة +15 عامًا في علاج أمراض القلب المعقدة.",
                    "Cairo", "القاهرة", "Zamalek", "01012345678",
                    "sarah.hassan@cliniq.com", "LIC-2010-001", "Cairo University", 15, 400, 4.9, 312, true,  true,
                    "https://i.pinimg.com/736x/4a/de/6d/4ade6d2a43e9bdf4cb20cf1dc05def79.jpg"),

                new SeedDoc("Dr. Ahmed Mansour",   "د. أحمد منصور",
                    "Neurology",    "طب الأعصاب",
                    "Consultant neurologist specializing in epilepsy and stroke management. Trained at Johns Hopkins.",
                    "استشاري أعصاب متخصص في الصرع وإدارة السكتة الدماغية.",
                    "Giza", "الجيزة", "Dokki", "01123456789",
                    "ahmed.mansour@cliniq.com", "LIC-2008-002", "Ain Shams University", 17, 500, 4.8, 265, true,  true,
                    "https://i.pinimg.com/736x/37/25/1e/37251eab10cb27e8b11515a3c78ce64d.jpg"),

                new SeedDoc("Dr. Layla Khalil",    "د. ليلى خليل",
                    "Pediatrics",   "طب الأطفال",
                    "Pediatric specialist with a warm approach to child healthcare. Expert in developmental delays and childhood immunization.",
                    "طبيبة أطفال متخصصة في تأخر النمو وتطعيمات الأطفال.",
                    "Cairo", "القاهرة", "Nasr City", "01234567890",
                    "layla.khalil@cliniq.com", "LIC-2012-003", "Cairo University", 12, 300, 4.7, 198, true,  true,
                    "https://i.pinimg.com/1200x/96/0a/92/960a9255469b48797e4c7e68640fd2dd.jpg"),

                new SeedDoc("Dr. Omar El-Sayed",   "د. عمر السيد",
                    "Orthopedics",  "العظام",
                    "Orthopedic surgeon specializing in sports injuries, joint replacement, and spine surgery.",
                    "جراح عظام متخصص في إصابات الرياضة وتركيب المفاصل وجراحة العمود الفقري.",
                    "Alexandria", "الإسكندرية", "Smouha", "01098765432",
                    "omar.elsayed@cliniq.com", "LIC-2009-004", "Alexandria University", 16, 450, 4.6, 143, true,  false,
                    "https://i.pinimg.com/1200x/54/0a/82/540a8268115c5c900e6d01301ed057bd.jpg"),

                new SeedDoc("Dr. Nadia Farouk",    "د. نادية فاروق",
                    "Dermatology",  "الأمراض الجلدية",
                    "Dermatologist and cosmetologist specializing in acne, hair loss, and laser treatments.",
                    "طبيبة جلدية وتجميل متخصصة في حب الشباب وتساقط الشعر والليزر.",
                    "Cairo", "القاهرة", "Heliopolis", "01187654321",
                    "nadia.farouk@cliniq.com", "LIC-2014-005", "Ain Shams University", 10, 350, 4.8, 421, true,  true,
                    "https://i.pinimg.com/736x/da/92/56/da9256b9faa75478af8c42b02d895e11.jpg"),

                new SeedDoc("Dr. Karim Naguib",    "د. كريم نجيب",
                    "Dentistry",    "طب الأسنان",
                    "Dental surgeon specializing in implants, orthodontics, and cosmetic dentistry. Over 5,000 successful procedures.",
                    "جراح أسنان متخصص في زراعة الأسنان والتقويم وطب الأسنان التجميلي.",
                    "Giza", "الجيزة", "6th of October", "01056789012",
                    "karim.naguib@cliniq.com", "LIC-2011-006", "Cairo University", 14, 250, 4.7, 389, true,  true,
                    "https://i.pinimg.com/736x/01/bc/83/01bc83577f3555e523ac2df3770b67b6.jpg"),

                new SeedDoc("Dr. Hana Saleh",      "د. هناء صالح",
                    "Gynecology",   "أمراض النساء",
                    "OB-GYN specialist with expertise in high-risk pregnancies, infertility, and minimally invasive surgery.",
                    "طبيبة نساء وتوليد متخصصة في الحمل عالي الخطورة والعقم والجراحة الطفيفة.",
                    "Cairo", "القاهرة", "Maadi", "01143210987",
                    "hana.saleh@cliniq.com", "LIC-2007-007", "Cairo University", 18, 500, 4.9, 287, true,  true,
                    "https://i.pinimg.com/736x/35/60/6f/35606f296e52742f503fb57360c9fafd.jpg"),

                new SeedDoc("Dr. Yusuf Ibrahim",   "د. يوسف إبراهيم",
                    "Psychiatry",   "الطب النفسي",
                    "Psychiatrist and psychotherapist specializing in anxiety, depression, and cognitive behavioral therapy.",
                    "طبيب نفسي ومعالج نفسي متخصص في القلق والاكتئاب والعلاج المعرفي السلوكي.",
                    "Giza", "الجيزة", "Maadi", "01076543210",
                    "yusuf.ibrahim@cliniq.com", "LIC-2013-008", "Ain Shams University", 12, 400, 4.3, 159, true,  true,
                    "https://i.pinimg.com/736x/38/72/3b/38723b0e575541da59c87601a7614a54.jpg"),

                new SeedDoc("Dr. Mona Zaki",       "د. منى زكي",
                    "Ophthalmology","طب العيون",
                    "Ophthalmologist specializing in LASIK, cataract surgery, and retinal diseases.",
                    "طبيبة عيون متخصصة في عمليات الليزك وإزالة الماء الأبيض وأمراض الشبكية.",
                    "Cairo", "القاهرة", "New Cairo", "01212345678",
                    "mona.zaki@cliniq.com", "LIC-2010-009", "Cairo University", 15, 600, 4.8, 203, true,  false,
                    "https://i.pinimg.com/736x/a0/4c/ed/a04ced80a5986ae4e71b5c4fffe22731.jpg"),

                new SeedDoc("Dr. Tarek Younes",    "د. طارق يونس",
                    "ENT",          "أنف وأذن وحنجرة",
                    "ENT consultant specializing in sinusitis, hearing loss, and voice disorders.",
                    "استشاري أنف وأذن وحنجرة متخصص في التهاب الجيوب والسمع واضطرابات الصوت.",
                    "Alexandria", "الإسكندرية", "Sidi Gaber", "01334567890",
                    "tarek.younes@cliniq.com", "LIC-2006-010", "Alexandria University", 19, 350, 4.5, 178, true,  true,
                    "https://i.pinimg.com/736x/5e/71/6c/5e716c2452994fd74c8dd75a6f4485cc.jpg"),
            };

            foreach (var s in seedDoctors)
            {
                var user = new User
                {
                    FullName     = s.FullName,
                    Email        = s.Email,
                    Phone        = s.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123456", workFactor: 12),
                    Role         = Roles.Doctor,
                    IsActive     = true,
                    CreatedAt    = DateTime.UtcNow,
                    ProfileImage = s.ProfileImage,   // ✅ الصورة بتتحفظ في الـ DB من أول ما الـ seed يشتغل
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                db.Doctors.Add(new Doctor
                {
                    UserId            = user.Id,
                    Specialty         = s.Specialty,
                    SpecialtyAr       = s.SpecialtyAr,
                    Bio               = s.Bio,
                    BioAr             = s.BioAr,
                    City              = s.City,
                    CityAr            = s.CityAr,
                    Area              = s.Area,
                    LicenseNumber     = s.LicenseNumber,
                    University        = s.University,
                    YearsOfExperience = s.Experience,
                    ConsultationFee   = s.Fee,
                    Rating            = s.Rating,
                    ReviewsCount      = s.Reviews,
                    IsVerified        = s.IsVerified,
                    IsAvailable       = s.IsAvailable,
                    CreatedAt         = DateTime.UtcNow,
                });
                await db.SaveChangesAsync();
                Console.WriteLine($"✅ Doctor seeded → {s.FullName} ({s.Specialty})");
            }

            Console.WriteLine("🎉 Database seeding complete!");
        }

        private record SeedDoc(
            string FullName, string FullNameAr,
            string Specialty, string SpecialtyAr,
            string Bio, string BioAr,
            string City, string CityAr, string Area,
            string Phone, string Email,
            string LicenseNumber, string University,
            int Experience, decimal Fee,
            double Rating, int Reviews,
            bool IsVerified, bool IsAvailable,
            string? ProfileImage = null);  // ✅ صورة كل دكتور
    }
}
