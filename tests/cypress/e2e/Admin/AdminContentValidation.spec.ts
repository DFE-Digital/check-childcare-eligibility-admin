const parentLastName = Cypress.env('lastName') || 'Tester';
const NIN = 'PN668767B';
const validNIN = 'NN123456C';
const invalidNIN = 'INVALID123';

const visitPrefilledForm = (onlyfill?: boolean) => {
    if (!onlyfill) {
        cy.visit("/Check/Enter_Details");
        cy.get('#LastName').should('exist');
    }

    cy.window().then(win => {
        const lastNameEl = win.document.getElementById('LastName') as HTMLInputElement;
        const dayEl = win.document.getElementById('Day') as HTMLInputElement;
        const monthEl = win.document.getElementById('Month') as HTMLInputElement;
        const yearEl = win.document.getElementById('Year') as HTMLInputElement;
        const ninEl = win.document.getElementById('NationalInsuranceNumber') as HTMLInputElement;

        if (lastNameEl) lastNameEl.value = parentLastName;
        if (dayEl) dayEl.value = '01';
        if (monthEl) monthEl.value = '01';
        if (yearEl) yearEl.value = '1990';
        if (ninEl) ninEl.value = NIN;
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
        cy.contains('button', '2 years old early learning').click();
        
        // We should now be on the Enter_Details page
        cy.url().should('include', '/Check/Enter_Details');
    });

    it('displays error messages for missing date fields', () => {
        // Fill in the last name to avoid that validation error
        cy.get('#LastName').type(parentLastName);
        
        // Fill in the NI number to avoid that validation error
        cy.get('#NationalInsuranceNumber').type(NIN);
        
        // Clear the date fields
        cy.get('#Day').clear();
        cy.get('#Month').clear();
        cy.get('#Year').clear();
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-summary__list').should('contain', "Enter parent or guardian's date of birth");
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
        cy.get('#LastName').clear().type(parentLastName);
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('2005');
        cy.get('#NationalInsuranceNumber').clear().type(NIN);
        cy.contains('Run check').click();

        // Wait for the loader page and then the result page
        cy.url().should('include', '/Check/Loader');
        cy.get('.govuk-notification-banner', { timeout: 29000 }).should('exist');
    });
});

describe('Last Name Validation Tests', () => {
    beforeEach(() => {
        // Start by logging in as LA type
        cy.checkSession('LA');
        
        // Navigate from Dashboard to SingleCheckMenu
        cy.visit('/');
        cy.contains('a', 'Run a check').click();
        
        // From SingleCheckMenu, select an option (e.g. 2 years old early learning)
        cy.contains('button', '2 years old early learning').click();
        
        // We should now be on the Enter_Details page
        cy.url().should('include', '/Check/Enter_Details');
    });

    it('displays error messages for missing last name', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN);
        
        // Leave last name empty
        cy.get('#LastName').clear();
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-message').should('contain', "Enter parent or guardian's last name in full");
        cy.get('#LastName').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for invalid last name characters', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN);
        
        // Enter invalid last name with special characters
        cy.get('#LastName').clear().type('Smith123@');
        
        // Submit the form
        cy.contains('Run check').click();

        // Check that the validation error appears
        cy.get('.govuk-error-summary').should('exist');
        cy.get('.govuk-error-message').should('contain', 'Enter a last name with valid characters');
        cy.get('#LastName').should('have.class', 'govuk-input--error');
    });

    it('allows valid last name submission', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN);
        
        // Enter valid last name (removing the backslash that was escaping the apostrophe)
        cy.get('#LastName').clear().type('Smith-O\'Brien');
        
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
        cy.contains('button', '2 years old early learning').click();
        
        // We should now be on the Enter_Details page
        cy.url().should('include', '/Check/Enter_Details');
    });

    it('displays error messages for missing NI number', () => {
        // Fill in valid date of birth
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('1990');
        
        // Fill in valid last name
        cy.get('#LastName').clear().type('Smith');
        
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
        
        // Fill in valid last name
        cy.get('#LastName').clear().type('Smith');
        
        // Enter invalid NI number
        cy.get('#NationalInsuranceNumber').clear().type(invalidNIN);
        
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
        
        // Fill in valid last name
        cy.get('#LastName').clear().type('Smith');
        
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
        
        // Fill in valid last name
        cy.get('#LastName').clear().type('Smith');
        
        // Enter valid NI number
        cy.get('#NationalInsuranceNumber').clear().type(validNIN);
        
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