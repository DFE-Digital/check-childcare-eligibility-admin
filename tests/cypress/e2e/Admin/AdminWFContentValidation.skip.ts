const eligibilityCode = "12345678900";
var NIN_WF = 'PN668767B';
const validNIN_WF = 'NN123456C';
const invalidNIN_WF = 'INVALID123';

const visitPrefilledFormWF = (onlyfill?: boolean) => {
    if (!onlyfill) {
        cy.visit("/Check/Enter_Details_WF");
        cy.get('#Child_EligibilityCode').should('exist');
    }

    cy.window().then(win => {
        const eligibilityCodeEl = win.document.getElementById('Child_EligibilityCode') as HTMLInputElement;
        const ninEl = win.document.getElementById('NationalInsuranceNumber') as HTMLInputElement;
        const dayEl = win.document.getElementById('Day') as HTMLInputElement;
        const monthEl = win.document.getElementById('Month') as HTMLInputElement;
        const yearEl = win.document.getElementById('Year') as HTMLInputElement;

        if (eligibilityCodeEl) eligibilityCodeEl.value = eligibilityCode;
        if (ninEl) ninEl.value = NIN_WF;
        if (dayEl) dayEl.value = '01';
        if (monthEl) monthEl.value = '01';
        if (yearEl) yearEl.value = '1990';
    });
};

describe('Date of Birth Validation Tests', () => {
    beforeEach(() => {
        cy.checkSession('LA');
        cy.visit(Cypress.config().baseUrl ?? "");
        
        // Navigate from Dashboard to SingleCheckMenu
        cy.visit('/');
        cy.contains('a', 'Run a check').click();
        
        // From SingleCheckMenu, select an option (e.g. 2 years old early learning)
        cy.contains('button', 'Childcare for working families').click();
        
        // We should now be on the Enter_Details page
        cy.url().should('include', '/Check/Enter_Details_WF');
    });

    it('displays error messages for missing date fields', () => {
        // Fill in the Eligibility Code to avoid that validation error
        cy.get('#Child_EligibilityCode').type(eligibilityCode);
        
        // Clear the date fields
        cy.get('#Day').clear();
        cy.get('#Month').clear();
        cy.get('#Year').clear();
        
        // Fill in the NI number to avoid that validation error
        cy.get('#NationalInsuranceNumber').type(NIN_WF);

        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-summary__list').should('contain', "Enter child's date of birth");
    });

    it('displays error messages for non-numeric inputs', () => {
        cy.get('#Day').clear().type('abc');
        cy.get('#Month').clear().type('xyz');
        cy.get('#Year').clear().type('abcd');
        cy.contains('Run check').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Month').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for out-of-range inputs', () => {
        cy.get('#Day').clear().type('50');
        cy.get('#Month').clear().type('13');
        cy.get('#Year').clear().type('1800');
        cy.contains('Run check').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Month').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for future dates', () => {
        cy.get('#Day').clear().type('01');
        cy.get('#Month').clear().type('01');
        cy.get('#Year').clear().type((new Date().getFullYear() + 1).toString());
        cy.contains('Run check').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be in the past');
    });

    it('displays error messages for invalid combinations', () => {
        cy.get('#Day').clear().type('31');
        cy.get('#Month').clear().type('02');
        cy.get('#Year').clear().type('2020');
        cy.contains('Run check').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
    });

    it('allows valid date of birth submission', () => {
        cy.get('#Child_EligibilityCode').clear().type(eligibilityCode);
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('2005');
        cy.get('#NationalInsuranceNumber').clear().type(NIN_WF);
        cy.contains('Run check').click();

        // Wait for the loader page and then the result page
        cy.url().should('include', '/Check/Loader');
        cy.get('.govuk-notification-banner', { timeout: 15000 }).should('exist');
    });
});

describe('Eligibility Code Validation Tests', () => {
    beforeEach(() => {
        // Start by logging in as LA type
        cy.checkSession('LA');
        
        // Navigate from Dashboard to SingleCheckMenu
        cy.visit('/');
        cy.contains('a', 'Run a check').click();
        
        // From SingleCheckMenu, select an option (e.g. 2 years old early learning)
        cy.contains('button', 'Childcare for working families').click();
        
        // We should now be on the Enter_Details page
        cy.url().should('include', '/Check/Enter_Details_WF');
    });

    it('displays error messages for missing Eligibility Code', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN_WF);
        
        // Leave Eligibility Code empty
        cy.get('#Child_EligibilityCode').clear();
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-message').should('contain', "Eligibility code is required");
        cy.get('#Child_EligibilityCode').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for invalid Eligibility Code characters', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN_WF);
        
        // Enter invalid Eligibility Code with special characters
        cy.get('#Child_EligibilityCode').clear().type('NotNum!');
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-message').should('contain', 'Eligibility code must only contain numbers');
        cy.get('#Child_EligibilityCode').should('have.class', 'govuk-input--error');
    });

        it('displays error messages for incorrect number of Eligibility Code characters', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN_WF);
        
        // Enter invalid length Eligibility Code
        cy.get('#Child_EligibilityCode').clear().type('1234');
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-message').should('contain', 'Eligibility code must be exactly 11 digits long');
        cy.get('#Child_EligibilityCode').should('have.class', 'govuk-input--error');
    });

    it('allows valid Eligibility Code submission', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN_WF);
        
        // Enter valid EligibilityCode
        cy.get('#Child_EligibilityCode').clear().type('12345678900');
        
        // Submit the form
        cy.contains('Run check').click();

        // Should proceed to next step without validation errors
        cy.contains('There is a problem').should('not.exist');
    });
});

describe('National Insurance Number Validation Tests', () => {
    beforeEach(() => {
        // Start by logging in as LA type
        cy.checkSession('LA');
        
        // Navigate from Dashboard to SingleCheckMenu
        cy.visit('/');
        cy.contains('a', 'Run a check').click();
        
        // From SingleCheckMenu, select an option (e.g. 2 years old early learning)
        cy.contains('button', 'Childcare for working families').click();
        
        // We should now be on the Enter_Details page
        cy.url().should('include', '/Check/Enter_Details_WF');
    });

    it('displays error messages for missing NI number', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid Eligibility Code
        cy.get('#Child_EligibilityCode').clear().type('12345678900');
        
        // Leave NI number empty
        cy.get('#NationalInsuranceNumber').clear();
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-message').should('contain', 'Enter a National Insurance number');
        cy.get('#NationalInsuranceNumber').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for invalid NI number format', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid Eligibility Code
        cy.get('#Child_EligibilityCode').clear().type('12345678900');
        
        // Enter invalid NI number
        cy.get('#NationalInsuranceNumber').clear().type(invalidNIN_WF);
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-message').should('contain', 'National Insurance number should contain no more than 9 alphanumeric characters');
        cy.get('#NationalInsuranceNumber').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for disallowed NI number prefixes', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid Eligibility Code
        cy.get('#Child_EligibilityCode').clear().type('12345678900');
        
        // Enter NI number with disallowed prefix
        cy.get('#NationalInsuranceNumber').clear().type('GB123456C');
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-message').should('contain', 'Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D');
        cy.get('#NationalInsuranceNumber').should('have.class', 'govuk-input--error');
    });

    it('allows valid NI number submission', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid Eligibility Code
        cy.get('#Child_EligibilityCode').clear().type('12345678900');
        
        // Enter valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN_WF);
        
        // Submit the form
        cy.contains('Run check').click();

        // Should proceed to next step without validation errors
        cy.contains('There is a problem').should('not.exist');
    });
});

describe("Navigation and links test", () => {
    beforeEach(() => {
        cy.checkSession('LA'); // if no session exists login as LA type
    });

    it("Dashboard should have run a check option", () => {
        cy.visit('/');
        cy.contains('a', 'Run a check').should('exist');
    });
});