using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcCoreAuth.Areas.Identity.Data;
using MvcCoreAuth.Authorization;
using MvcCoreAuth.Data;
using MvcCoreAuth.Models;

namespace MvcCoreAuth.Controllers
{
    public class ContactsController : Controller
    {
        protected IAuthorizationService AuthorizationService { get; }
        protected UserManager<MvcCoreAuthUser> UserManager { get; }
        protected ApplicationDbContext Context { get; }

        public ContactsController(ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<MvcCoreAuthUser> userManager)
        {
            Context = context;
            UserManager = userManager;
            AuthorizationService = authorizationService;
        }

        // GET: Contacts
        public async Task<IActionResult> Index()
        {
            var contacts = from c in Context.Contact
                           select c;

            var isAuthorized = User.IsInRole(Constants.ContactManagersRole) ||
                               User.IsInRole(Constants.ContactAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved contacts are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                contacts = contacts.Where(c => c.Status == ContactStatus.Approved
                                            || c.OwnerID == currentUserId);
            }
            
            return View(await contacts.ToListAsync());
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await Context.Contact
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = User.IsInRole(Constants.ContactManagersRole) ||
                           User.IsInRole(Constants.ContactAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized
                && currentUserId != contact.OwnerID
                && contact.Status != ContactStatus.Approved)
            {
                return new ChallengeResult();
            }

            return View(contact);
        }

        //POST: Contacts/Details

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, ContactStatus status)
        {
            var contact = await Context.Contact.FirstOrDefaultAsync(
                                                  m => m.ContactId == id);

            if (contact == null)
            {
                return NotFound();
            }

            var contactOperation = (status == ContactStatus.Approved)
                                                       ? ContactOperations.Approve
                                                       : ContactOperations.Reject;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, contact,
                                        contactOperation);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }
            contact.Status = status;
            Context.Contact.Update(contact);
            await Context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContactId,OwnerID,Name,Address,City,State,Zip,Email,Status")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.OwnerID = UserManager.GetUserId(User);
                var isAuthorized = await AuthorizationService.AuthorizeAsync(User, contact, ContactOperations.Create);
                if (!isAuthorized.Succeeded)
                {
                    return new ChallengeResult();
                }

                Context.Add(contact);
                await Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await Context.Contact.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, contact, ContactOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContactId,OwnerID,Name,Address,City,State,Zip,Email,Status")] Contact contact)
        {
            if (id != contact.ContactId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch Contact from DB to get OwnerID.
                    var contactDb = await Context
                        .Contact.AsNoTracking()
                        .FirstOrDefaultAsync(m => m.ContactId == id);

                    if (contactDb == null)
                    {
                        return NotFound();
                    }

                    var isAuthorized = await AuthorizationService.AuthorizeAsync( User, contactDb, ContactOperations.Update);
                    if (!isAuthorized.Succeeded)
                    {
                        return new ChallengeResult();
                    }
                    contact.OwnerID = contactDb.OwnerID;

                    Context.Attach(contact).State = EntityState.Modified;

                    if (contactDb.Status == ContactStatus.Approved)
                    {
                        // If the contact is updated after approval, 
                        // and the user cannot approve,
                        // set the status back to submitted so the update can be
                        // checked and approved.
                        var canApprove = await AuthorizationService.AuthorizeAsync(User,
                                                contact,
                                                ContactOperations.Approve);

                        if (!canApprove.Succeeded)
                        {
                            contact.Status = ContactStatus.Submitted;
                        }
                    }

                    Context.Update(contact);
                    await Context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.ContactId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await Context.Contact
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                User, contact,
                                                ContactOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await Context.Contact.FindAsync(id);

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                 User, contact,
                                                 ContactOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            Context.Contact.Remove(contact);
            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return Context.Contact.Any(e => e.ContactId == id);
        }
    }
}
