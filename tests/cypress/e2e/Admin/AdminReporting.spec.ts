describe('Full journey of checking reporting in LA portal', () => {
    const validEligibilityCode = '12345678901'; // Example eligibility code for testing
    const invalidEligibilityCode = '00'; // Example invalid eligibility code for testing  
    const invalidEligibilityCodeletters = 'abc123'; // Example invalid code for testing
    
    beforeEach(() => {
        // Login with LA session
        cy.checkSession('LA');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home")
        cy.wait(1);
        cy.get('h1').should('include.text', 'Manage eligibility for childcare support');
    });

    it('Allows an LA user to run a successful report', () => {
        cy.contains('Run reports').click();
        cy.get('h1').should('include.text', 'Run reports');
        cy.contains('a', 'View eligibility code history').click();
        cy.get('h1').should('include.text', 'View eligibility code history');
        cy.get('#EligibilityCode').type(validEligibilityCode);
        cy.contains('button', 'View code history').click();
        cy.get('h1').should('include.text', 'History for eligibility code 12345678901');
    });
    it('Will return an error message from an invalid eligibility code', () => {
        cy.contains('Run reports').click();
        cy.get('h1').should('include.text', 'Run reports');
        cy.contains('a', 'View eligibility code history').click();
        cy.get('h1').should('include.text', 'View eligibility code history');
        cy.get('#EligibilityCode').type(invalidEligibilityCode);
        cy.contains('button', 'View code history').click();
        cy.get('.govuk-error-message').should('contain.text', 'Eligibility code must be 11 digits long');
    });
    it('Will return an error message from an invalid eligibility code with letters', () => {
        cy.contains('Run reports').click();
        cy.get('h1').should('include.text', 'Run reports');
        cy.contains('a', 'View eligibility code history').click();
        cy.get('h1').should('include.text', 'View eligibility code history');
        cy.get('#EligibilityCode').type(invalidEligibilityCodeletters);
        cy.contains('button', 'View code history').click();
        cy.get('.govuk-error-message').should('contain.text', 'Eligibility code must only contain numbers');
    });
});