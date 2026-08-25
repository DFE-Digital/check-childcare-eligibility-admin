import * as testNino from 'test-nino'; //for generating unique NI numbers until we have a way to delete existing records

describe('Foster Families search records and add Family', () => {
    const nino = '000000000' //Initially set as something obviously wrong
    const carer = {
        firstName: 'Testing',
        lastName: 'Tester',
        dobDay: '1',
        dobMonth: '2',
        dobYear: '1993',
        displayedDob: '1 February 1993',
        nin: nino,
        hasPartner: true
    };
    const partner = {
        firstName: 'Partnering',
        lastName: 'Partner',
        dobDay: '4',
        dobMonth: '5',
        dobYear: '1996',
        displayedDob: '4 May 1996',
        nin: 'NN123456C'
    };
    const child = {
        firstName: 'Childing',
        lastName: 'Child',
        postCode: 'CH1 1LD',
        dobDay: '7',
        dobMonth: '8',
        dobYear: '2022',
        displayedDob: '7 August 2022'
    };
    const now = new Date();
    const validAfterDate = new Date(now);
    validAfterDate.setDate(validAfterDate.getDate() - 31); // Today minus 31 days for validation message
    const submittedDate = {
        submittedDay: now.getDate().toString(),
        submittedMonth: (now.getMonth() + 1).toString(), // JS months are 0-indexed
        submittedYear: now.getFullYear().toString(),
        displayedSubmittedDate: now.toLocaleDateString('en-GB', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        }),
        validAfterDateDay: validAfterDate.getDate().toString(),
        validAfterDateMonth: (validAfterDate.getMonth() + 1).toString(), // JS months are 0-indexed
        validAfterDateYear: validAfterDate.getFullYear().toString(),
        displayedvalidAfterDate: validAfterDate.toLocaleDateString('en-GB', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        })
    };

    beforeEach(() => {
        // Login with LA session
        cy.checkSession('LA');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home")
        cy.wait(1);
        cy.get('h1').should('include.text', 'Manage eligibility for childcare support');
    });

    it('Allows an LA user to navigate; to the Foster Families search page and create a new Foster Family record', () => {
        //Navigate to run a check
        cy.contains('Manage foster families').click();
        cy.contains('Add family and create code').click();


        //CARER FORM ----------------------------------------------------------------------------
        //Carer form validation tests
        cy.get('h1').should('include.text', 'Enter the carer details');
        cy.url().should('include', 'FosterFamilies/Carer');
        carer.nin = testNino.random(); // Returns a valid UK National Insurance number e.g. AA000000A
        
        //Submit empty form to check for 'required' validation messages
        cy.contains('button', 'Continue').click();
        cy.get('#CarerFirstName').should('have.class', 'govuk-input--error');
        cy.get('#CarerLastName').should('have.class', 'govuk-input--error');
        cy.get('#CarerDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#CarerNationalInsuranceNumber').should('have.class', 'govuk-input--error');
        cy.get('#HasPartner').should('have.class', 'input-validation-error');
        cy.get('.govuk-error-message').should('contain', "Enter carer's first name");
        cy.get('.govuk-error-message').should('contain', "Enter carer's last name");
        cy.get('.govuk-error-message').should('contain', "Enter carer's date of birth");
        cy.get('.govuk-error-message').should('contain', "Enter a National Insurance number");
        cy.get('.govuk-error-message').should('contain', "Select yes if the carer has a partner");

        //-- Name --//
        //Invalid characters entered
        cy.get('#CarerFirstName').type("12345");
        cy.get('#CarerLastName').type("12345");
        cy.contains('button', 'Continue').click();
        cy.get('#CarerFirstName').should('have.class', 'govuk-input--error');
        cy.get('#CarerLastName').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Carer's first name must only include letters a to z, and special characters such as hyphens, spaces and apostrophes");
        cy.get('.govuk-error-message').should('contain', "Carer's last name must only include letters a to z, and special characters such as hyphens, spaces and apostrophes");

        //-- Date of birth --//
        //no day
        cy.get('#CarerDateOfBirth\\.Day').clear()
        cy.get('#CarerDateOfBirth\\.Month').clear().type(carer.dobMonth);
        cy.get('#CarerDateOfBirth\\.Year').clear().type(carer.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#CarerDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a day");

        //no month
        cy.get('#CarerDateOfBirth\\.Day').clear().type(carer.dobDay);
        cy.get('#CarerDateOfBirth\\.Month').clear()
        cy.get('#CarerDateOfBirth\\.Year').clear().type(carer.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#CarerDateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a month");

        //no year
        cy.get('#CarerDateOfBirth\\.Day').clear().type(carer.dobDay);
        cy.get('#CarerDateOfBirth\\.Month').clear().type(carer.dobMonth);
        cy.get('#CarerDateOfBirth\\.Year').clear()
        cy.contains('button', 'Continue').click();
        cy.get('#CarerDateOfBirth\\.Year').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a year");

        //year incomplete
        cy.get('#CarerDateOfBirth\\.Day').clear().type(carer.dobDay);
        cy.get('#CarerDateOfBirth\\.Month').clear().type(carer.dobMonth);
        cy.get('#CarerDateOfBirth\\.Year').clear().type("19");
        cy.contains('button', 'Continue').click();
        cy.get('#CarerDateOfBirth\\.Year').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Year must include 4 numbers");

        //date in future
        cy.get('#CarerDateOfBirth\\.Day').clear().type(carer.dobDay);
        cy.get('#CarerDateOfBirth\\.Month').clear().type(carer.dobMonth);
        cy.get('#CarerDateOfBirth\\.Year').clear().type((now.getFullYear() + 1).toString());
        cy.contains('button', 'Continue').click();
        cy.get('#CarerDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must be in the past");

        //impossible date
        cy.get('#CarerDateOfBirth\\.Day').clear().type("35");
        cy.get('#CarerDateOfBirth\\.Month').clear().type(carer.dobMonth);
        cy.get('#CarerDateOfBirth\\.Year').clear().type(carer.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#CarerDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must be a real date");

        //-- National Insurance Number --//
        cy.get('#CarerNationalInsuranceNumber').clear().type("1111111111");
        cy.contains('button', 'Continue').click();
        cy.get('#CarerNationalInsuranceNumber').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Enter a National Insurance number in the correct format");

        //Add legit Carer details
        cy.get('h1').should('include.text', 'Enter the carer details');
        cy.url().should('include', 'FosterFamilies/Carer');
        cy.get('#CarerFirstName').clear().type(carer.firstName);
        cy.get('#CarerLastName').clear().type(carer.lastName);
        cy.get('#CarerDateOfBirth\\.Day').clear().type(carer.dobDay);
        cy.get('#CarerDateOfBirth\\.Month').clear().type(carer.dobMonth);
        cy.get('#CarerDateOfBirth\\.Year').clear().type(carer.dobYear);
        cy.get('#CarerNationalInsuranceNumber').clear().type(carer.nin);
        cy.get(`input[name="HasPartner"][value="${carer.hasPartner ? 'True' : 'False'}"]`).check();
        cy.contains('button', 'Continue').click();


        //PARTNER FORM ----------------------------------------------------------------------------
        //Partner form validation tests
        cy.get('h1').should('include.text', 'Enter the partner details');
        cy.url().should('include', 'FosterFamilies/Partner');

        //Submit empty form to check for 'required' validation messages
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerFirstName').should('have.class', 'govuk-input--error');
        cy.get('#PartnerLastName').should('have.class', 'govuk-input--error');
        cy.get('#PartnerDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#PartnerNationalInsuranceNumber').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Enter partner's first name");
        cy.get('.govuk-error-message').should('contain', "Enter partner's last name");
        cy.get('.govuk-error-message').should('contain', "Enter partner's date of birth");
        cy.get('.govuk-error-message').should('contain', "Enter a National Insurance number");

        //-- Name --//
        //Invalid characters entered
        cy.get('#PartnerFirstName').type("12345");
        cy.get('#PartnerLastName').type("12345");
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerFirstName').should('have.class', 'govuk-input--error');
        cy.get('#PartnerLastName').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Partner's first name must only include letters a to z, and special characters such as hyphens, spaces and apostrophes");
        cy.get('.govuk-error-message').should('contain', "Partner's last name must only include letters a to z, and special characters such as hyphens, spaces and apostrophes");

        //-- Date of birth --//
        //no day
        cy.get('#PartnerDateOfBirth\\.Day').clear()
        cy.get('#PartnerDateOfBirth\\.Month').clear().type(partner.dobMonth);
        cy.get('#PartnerDateOfBirth\\.Year').clear().type(partner.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a day");

        //no month
        cy.get('#PartnerDateOfBirth\\.Day').clear().type(partner.dobDay);
        cy.get('#PartnerDateOfBirth\\.Month').clear()
        cy.get('#PartnerDateOfBirth\\.Year').clear().type(partner.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerDateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a month");

        //no year
        cy.get('#PartnerDateOfBirth\\.Day').clear().type(partner.dobDay);
        cy.get('#PartnerDateOfBirth\\.Month').clear().type(partner.dobMonth);
        cy.get('#PartnerDateOfBirth\\.Year').clear()
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerDateOfBirth\\.Year').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a year");

        //year incomplete
        cy.get('#PartnerDateOfBirth\\.Day').clear().type(partner.dobDay);
        cy.get('#PartnerDateOfBirth\\.Month').clear().type(partner.dobMonth);
        cy.get('#PartnerDateOfBirth\\.Year').clear().type("19");
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerDateOfBirth\\.Year').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Year must include 4 numbers");

        //date in future
        cy.get('#PartnerDateOfBirth\\.Day').clear().type(partner.dobDay);
        cy.get('#PartnerDateOfBirth\\.Month').clear().type(partner.dobMonth);
        cy.get('#PartnerDateOfBirth\\.Year').clear().type((now.getFullYear() + 1).toString());
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must be in the past");

        //impossible date
        cy.get('#PartnerDateOfBirth\\.Day').clear().type("35");
        cy.get('#PartnerDateOfBirth\\.Month').clear().type(partner.dobMonth);
        cy.get('#PartnerDateOfBirth\\.Year').clear().type(partner.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must be a real date");

        //-- National Insurance Number --//
        cy.get('#PartnerNationalInsuranceNumber').clear().type("1111111111");
        cy.contains('button', 'Continue').click();
        cy.get('#PartnerNationalInsuranceNumber').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Enter a National Insurance number in the correct format");

        // Add legit Partner details
        cy.get('h1').should('include.text', 'Enter the partner details');
        cy.url().should('include', 'FosterFamilies/Partner');
        cy.get('#PartnerFirstName').clear().type(partner.firstName);
        cy.get('#PartnerLastName').clear().type(partner.lastName);
        cy.get('#PartnerDateOfBirth\\.Day').clear().type(partner.dobDay);
        cy.get('#PartnerDateOfBirth\\.Month').clear().type(partner.dobMonth);
        cy.get('#PartnerDateOfBirth\\.Year').clear().type(partner.dobYear);
        cy.get('#PartnerNationalInsuranceNumber').clear().type(partner.nin);
        cy.contains('button', 'Continue').click();

        //CHILD FORM ----------------------------------------------------------------------------
        //Child form validation tests
        cy.get('h1').should('include.text', 'Enter the child details');
        cy.url().should('include', 'FosterFamilies/Child');

        //Submit empty form to check for 'required' validation messages
        cy.contains('button', 'Continue').click();
        cy.get('#ChildFirstName').should('have.class', 'govuk-input--error');
        cy.get('#ChildLastName').should('have.class', 'govuk-input--error');
        cy.get('#ChildDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#ChildPostCode').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Enter child's first name");
        cy.get('.govuk-error-message').should('contain', "Enter child's last name");
        cy.get('.govuk-error-message').should('contain', "Enter child's date of birth");
        cy.get('.govuk-error-message').should('contain', "Enter postcode");

        //-- Name --//
        //Invalid characters entered
        cy.get('#ChildFirstName').type("12345");
        cy.get('#ChildLastName').type("12345");
        cy.contains('button', 'Continue').click();
        cy.get('#ChildFirstName').should('have.class', 'govuk-input--error');
        cy.get('#ChildLastName').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Child's first name must only include letters a to z, and special characters such as hyphens, spaces and apostrophes");
        cy.get('.govuk-error-message').should('contain', "Child's last name must only include letters a to z, and special characters such as hyphens, spaces and apostrophes");

        //-- Date of birth --//
        //no day
        cy.get('#ChildDateOfBirth\\.Day').clear()
        cy.get('#ChildDateOfBirth\\.Month').clear().type(child.dobMonth);
        cy.get('#ChildDateOfBirth\\.Year').clear().type(child.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#ChildDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a day");

        //no month
        cy.get('#ChildDateOfBirth\\.Day').clear().type(child.dobDay);
        cy.get('#ChildDateOfBirth\\.Month').clear()
        cy.get('#ChildDateOfBirth\\.Year').clear().type(child.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#ChildDateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a month");

        //no year
        cy.get('#ChildDateOfBirth\\.Day').clear().type(child.dobDay);
        cy.get('#ChildDateOfBirth\\.Month').clear().type(child.dobMonth);
        cy.get('#ChildDateOfBirth\\.Year').clear()
        cy.contains('button', 'Continue').click();
        cy.get('#ChildDateOfBirth\\.Year').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must include a year");

        //year incomplete
        cy.get('#ChildDateOfBirth\\.Day').clear().type(child.dobDay);
        cy.get('#ChildDateOfBirth\\.Month').clear().type(child.dobMonth);
        cy.get('#ChildDateOfBirth\\.Year').clear().type("19");
        cy.contains('button', 'Continue').click();
        cy.get('#ChildDateOfBirth\\.Year').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Year must include 4 numbers");

        //date in future
        cy.get('#ChildDateOfBirth\\.Day').clear().type(child.dobDay);
        cy.get('#ChildDateOfBirth\\.Month').clear().type(child.dobMonth);
        cy.get('#ChildDateOfBirth\\.Year').clear().type((now.getFullYear() + 1).toString());
        cy.contains('button', 'Continue').click();
        cy.get('#ChildDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must be in the past");

        //impossible date
        cy.get('#ChildDateOfBirth\\.Day').clear().type("35");
        cy.get('#ChildDateOfBirth\\.Month').clear().type(child.dobMonth);
        cy.get('#ChildDateOfBirth\\.Year').clear().type(child.dobYear);
        cy.contains('button', 'Continue').click();
        cy.get('#ChildDateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Date of birth must be a real date");

        //-- Post Code --//
        cy.get('#ChildPostCode').clear().type("1111111111");
        cy.contains('button', 'Continue').click();
        cy.get('#ChildPostCode').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Enter a full UK postcode");

        // Add legit Child details
        cy.get('h1').should('include.text', 'Enter the child details');
        cy.url().should('include', 'FosterFamilies/Child');
        cy.get('#ChildFirstName').clear().type(child.firstName);
        cy.get('#ChildLastName').clear().type(child.lastName);
        cy.get('#ChildDateOfBirth\\.Day').clear().type(child.dobDay);
        cy.get('#ChildDateOfBirth\\.Month').clear().type(child.dobMonth);
        cy.get('#ChildDateOfBirth\\.Year').clear().type(child.dobYear);
        cy.get('#ChildPostCode').clear().type(child.postCode);
        cy.contains('button', 'Continue').click();


        //SUBMITTED DATE FORM ----------------------------------------------------------------------------
        //Submitted Date form validation tests
        cy.get('h1').should('include.text', 'When was the application submitted?');
        cy.url().should('include', 'FosterFamilies/SubmittedDate');

        //Submit empty form to check for 'required' validation messages
        cy.contains('button', 'Continue').click();
        cy.get('#submission-date-radios-Today').should('have.class', 'input-validation-error');
        cy.get('.govuk-error-message').should('contain', "Select whether to use today's date or another date");

        //Another Date selected but not filled
        cy.get('#submission-date-radios-AnotherDate').check();
        cy.contains('button', 'Continue').click();
        cy.get('#SubmissionDate\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Enter application submitted on date");

        //-- Another Date, date validation --//
        //no day
        cy.get('#SubmissionDate\\.Day').clear()
        cy.get('#SubmissionDate\\.Month').clear().type(submittedDate.submittedMonth);
        cy.get('#SubmissionDate\\.Year').clear().type(submittedDate.submittedYear);
        cy.contains('button', 'Continue').click();
        cy.get('#SubmissionDate\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Application submitted on date must include a day");

        //no month
        cy.get('#SubmissionDate\\.Day').clear().type(submittedDate.submittedDay);
        cy.get('#SubmissionDate\\.Month').clear();
        cy.get('#SubmissionDate\\.Year').clear().type(submittedDate.submittedYear);
        cy.contains('button', 'Continue').click();
        cy.get('#SubmissionDate\\.Month').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Application submitted on date must include a month");

        //no year
        cy.get('#SubmissionDate\\.Day').clear().type(submittedDate.submittedDay);
        cy.get('#SubmissionDate\\.Month').clear().type(submittedDate.submittedMonth);
        cy.get('#SubmissionDate\\.Year').clear();
        cy.contains('button', 'Continue').click();
        cy.get('#SubmissionDate\\.Year').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Application submitted on date must include a year");

        //year incomplete
        cy.get('#SubmissionDate\\.Day').clear().type(submittedDate.submittedDay);
        cy.get('#SubmissionDate\\.Month').clear().type(submittedDate.submittedMonth);
        cy.get('#SubmissionDate\\.Year').clear().type("19");
        cy.contains('button', 'Continue').click();
        cy.get('#SubmissionDate\\.Year').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Year must include 4 numbers");

        //date in future
        cy.get('#SubmissionDate\\.Day').clear().type(submittedDate.submittedDay);
        cy.get('#SubmissionDate\\.Month').clear().type(submittedDate.submittedMonth);
        cy.get('#SubmissionDate\\.Year').clear().type((now.getFullYear() + 1).toString());
        cy.contains('button', 'Continue').click();
        cy.get('#SubmissionDate\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Application submitted on date must be in the past");

        //impossible date
        cy.get('#SubmissionDate\\.Day').clear().type("35");
        cy.get('#SubmissionDate\\.Month').clear().type(submittedDate.submittedMonth);
        cy.get('#SubmissionDate\\.Year').clear().type(submittedDate.submittedYear);
        cy.contains('button', 'Continue').click();
        cy.get('#SubmissionDate\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "Application submitted on date must be a real date");

        //Too far in the past
        cy.get('#SubmissionDate\\.Day').clear().type(submittedDate.validAfterDateDay);
        cy.get('#SubmissionDate\\.Month').clear().type(submittedDate.validAfterDateMonth);
        cy.get('#SubmissionDate\\.Year').clear().type(submittedDate.validAfterDateYear);
        cy.contains('button', 'Continue').click();
        cy.get('#SubmissionDate\\.Day').should('have.class', 'govuk-input--error');
        cy.get('.govuk-error-message').should('contain', "The application submitted on date must be after " + submittedDate.displayedvalidAfterDate);

        //Add legit Submitted Date details
        cy.get('h1').should('include.text', 'When was the application submitted?');
        cy.url().should('include', 'FosterFamilies/SubmittedDate');
        cy.get('input[name="IsTodaySelected"][value="true"]').check();
        cy.contains('button', 'Continue').click();


        //Verify details are displayed
        cy.get('h1').should('include.text', 'Check details');
        cy.url().should('include', 'FosterFamilies/CheckDetails');

        //Carer
        cy.contains('.govuk-summary-card__title', 'Carer')
            .closest('.govuk-summary-card')
            .within(() => {
                cy.contains('.govuk-summary-list__key', 'Full name')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', `${carer.firstName} ${carer.lastName}`)
                cy.contains('.govuk-summary-list__key', 'Date of birth')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', carer.displayedDob)
                cy.contains('.govuk-summary-list__key', 'National Insurance number')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', carer.nin)
            })

        //Partner
        cy.contains('.govuk-summary-card__title', 'Partner')
            .closest('.govuk-summary-card')
            .within(() => {
                cy.contains('.govuk-summary-list__key', 'Full name')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', `${partner.firstName} ${partner.lastName}`)
                cy.contains('.govuk-summary-list__key', 'Date of birth')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', partner.displayedDob)
                cy.contains('.govuk-summary-list__key', 'National Insurance number')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', partner.nin)
            })

        //Child
        cy.contains('.govuk-summary-card__title', 'Child')
            .closest('.govuk-summary-card')
            .within(() => {
                cy.contains('.govuk-summary-list__key', 'Full name')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', `${child.firstName} ${child.lastName}`)
                cy.contains('.govuk-summary-list__key', 'Date of birth')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', child.displayedDob)
                cy.contains('.govuk-summary-list__key', 'Postcode')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', child.postCode)
            })

        //Submitted Date
        cy.contains('.govuk-summary-card__title', 'Eligibility code dates')
            .closest('.govuk-summary-card')
            .within(() => {

                cy.contains('.govuk-summary-list__key', 'Application submitted on')
                    .siblings('.govuk-summary-list__value')
                    .should('contain.text', submittedDate.displayedSubmittedDate)
            })
        cy.contains('button', 'Add family and create code').click();

        //Family added page
        cy.get('.govuk-panel--confirmation', { timeout: 10000 }).should('be.visible');
        cy.get('h1').should('include.text', 'Family added and code created');
        cy.url().should('include', 'FosterFamilies/CodeCreated');
        cy.get('.govuk-panel__body strong').invoke('text').should('match', /^\d+$/);
        cy.get('table.govuk-table').should('contain', `${child.firstName} ${child.lastName}`);

    });
});