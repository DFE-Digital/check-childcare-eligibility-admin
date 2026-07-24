describe('Test that approved accented characters are accepted in name input fields', () => {
    const parentLastName = 'Tester';
    const parentNinoEligible = "nn123456c";
    const parentNinoNotEligible = 'PN123456C'; // Updated to the specified NI number
    const parentNinoParentNotFound = 'RA123456C'; // NI number for parent not found scenario
    const parentLastNameNotFound = 'ttySimpson'; // Last name for parent not found scenario
    const parentNinoTechnicalError = 'AA668767B'; // Using an unusual NI number to potentially trigger errors

    it('Parent first and last names on Enter_Details should accept approved accented characters', () => {
        //Setup - Get to Enter_Details page to perform test
        // Login with LA session
        cy.checkSession('LA');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home")
        cy.wait(1);
        cy.get('h1').should('include.text', 'Manage eligibility for childcare support');
        cy.contains('Run a check').click();
        // Select 2YO eligibility type
        cy.get('h1').should('include.text', 'Run a check for one parent or guardian');
        cy.contains('button', 'Early learning for 2-year-olds').click();
        cy.url().should('include', '/Check/Enter_Details');
        //First and Last name fields should accept
        // [("OBrien", "plain letters")]
        // [("O'Brien", "straight apostrophe (U+0027)")]
        // [("O\u2019Brien", "right curly apostrophe (U+2019)")]
        // [("O\u2018Brien", "left curly apostrophe (U+2018)")]
        // [("Smith-Jones", "hyphen")]
        // [("St. Claire", "period and space")]
        // [("van den Berg", "spaces")]
        // [("ÁáÉéÍíÓóÚúÝýĆćĹĺŃńŔŕŚśŹź", "acute")]
        // [("ÀàÈèÌìÒòÙùẀẁỲỳ", "grave")]
        // [("ÂâÊêÎîÔôÛûĈĉĜĝĤĥĴĵŜŝŴŵŶŷ", "circumflex")]
        // [("ÃãÑñÕõĨĩŨũẼẽỸỹ", "tilde")]
        // [("ÄäËëÏïÖöÜüŸÿ", "umlaut or diaeresis")]
        // [("ÇçĢģĶķĻļŅņŖŗŞşŢţ", "cedilla")]
        // [("ÅåŮů", "ring")]
        // [("ĀāĒēĪīŌōŪūȲȳ", "macron")]
        // [("ĂăĔĕĞğĬĭŎŏŬŭ", "breve")]
        // [("ĊċĖėĠġİẊẋŻż", "dot above")]
        // [("ĄąĘęĮįŲų", "ogonek")]
        // [("ŐőŰű", "double acute")]
        cy.get('#LastName').type('OBrienO\'BrienO\u2019BrienO\u2018BrienSmith-JonesSt. Clairevan den BergÁáÉéÍíÓóÚúÝýĆćĹĺŃńŔŕŚśŹźÀàÈèÌìÒòÙùẀẁỲỳÀàÈèÌìÒòÙùẀẁỲỳÃãÑñÕõĨĩŨũẼẽỸỹÄäËëÏïÖöÜüŸÿÇçĢģĶķĻļŅņŖŗŞşŢţÅåŮůĀāĒēĪīŌōŪūȲȳĂăĔĕĞğĬĭŎŏŬŭĊċĖėĠġİẊẋŻżĄąĘęĮįŲųŐőŰű');
        cy.contains('button', 'Run check').click();
        cy.get('#error-summary')
            .should('not.contain.text', 'Enter a last name with valid characters');
    });
});
