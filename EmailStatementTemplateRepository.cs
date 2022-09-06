using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.MasterModel;
using DataAccess.DataModels;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    public class EmailStatementTemplateRepository : IEmailStatementTemplateRepository
    {
        private EntityModel _context = null;
        public EmailStatementTemplateRepository(EntityModel model)
        {
            this._context = model;
        }


        public void AddEmailStatementTemplate(EmailStatementTemplate emailStatementTemplate)
        {
            _context.EmailStatementTemplate.Add(emailStatementTemplate);
        }

        public List<EmailStatementTemplate> GetAllEmailTemplatesForCategory(int categoryId)
        {
            return _context.EmailStatementTemplate.Where(x => x.IsDeleted != true && x.CategoryId == categoryId).ToList();
        }

        public List<DefaultFormat> GetAllStatementFormatsForOrganization()
        {
            return _context.DefaultFormat.Where(x => x.IsDeleted != true && x.FormatFor == (int)FormatModuleName.StatementFormat).ToList();
        }

        public DefaultFormat GetStatementFormatById(int statementFormatId)
        {
            return _context.DefaultFormat.Where(x => x.FormatID == statementFormatId && x.IsDeleted != true).FirstOrDefault();
        }

        public EmailStatementTemplate GetEmailTemplateByCategoryIdAndName(int categoryId, string templateName)
        {
            return _context.EmailStatementTemplate.Where(x => x.IsDeleted != true && x.CategoryId == categoryId && x.TemplateName == templateName).FirstOrDefault();
        }

        public EmailStatementTemplate GetEmailTemplateByCategoryIdAndNameAndType(int categoryId, string templateName, int templateType)
        {
            return _context.EmailStatementTemplate.Where(x => x.IsDeleted != true && x.CategoryId == categoryId && x.TemplateName == templateName && x.TemplateFor == templateType).FirstOrDefault();
        }

        public void SaveTemplateAttachment(TemplateAttachment templateAttachment)
        {
            _context.TemplateAttachment.Add(templateAttachment);
        }

        public TemplateAttachment GetTemplateAttachment(int attachmentId, int templateId, int organizatioId)
        {
            return _context.TemplateAttachment.Where(x => x.AttachmentId == attachmentId && x.OrganizationId == organizatioId).FirstOrDefault();
        } 
        public List<TemplateAttachment> GetAlltemplateAttachmentForOrganization(int organizatioId)
        {
            return _context.TemplateAttachment.Where(x => x.OrganizationId == organizatioId&&x.IsDeleted!=true).ToList();
        }

        public OrganizationUserFormatPreference GetDefaultFormatForUser(int userId, string module)
        {
            return _context.OrganizationUserFormatPreference.Where(x => x.UserId == userId && x.ModuleName == module).FirstOrDefault();
        }

        public void AddOrganizationUserPreference(OrganizationUserFormatPreference orgUserPreference)
        {
            _context.OrganizationUserFormatPreference.Add(orgUserPreference);
        }

        
    }
}
