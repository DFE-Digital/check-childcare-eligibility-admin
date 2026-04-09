/**
 * Tests for the "Childcare for working families" tile visibility based on the
 * FeatureFlags:LAsThatCanUseWF configuration setting.
 *
 * Telford and Wrekin Council (establishment number 894) is not in the allowed list,
 * so it should NOT see the WF tile. Manchester City Council (352) is in the allowed
 * list, so it should see the WF tile.
 *
 * Both the single check menu (/home → Run a check) and the batch check menu
 * (/home → Run a batch check) apply the same feature flag logic.
 */

const WF_TILE_TEXT = 'Childcare for working families';
const TWO_YO_TILE_TEXT = 'Early learning for 2-year-olds';
const EYPP_TILE_TEXT = 'Early years pupil premium';

describe('Single check menu — Telford LA (WF disabled)', () => {
    beforeEach(() => {
        cy.loginLocalAuthorityUser();
        cy.visit('/home');
        cy.contains('a', 'Run a check').click();
        cy.url().should('include', '/Home/MenuSingleCheck');
    });

    it('shows the "Childcare for working families" tile', () => {
        cy.contains('button', WF_TILE_TEXT).should('not.exist');
    });

    it('shows the "Early learning for 2-year-olds" tile', () => {
        cy.contains('button', TWO_YO_TILE_TEXT).should('exist');
    });

    it('shows the "Early years pupil premium" tile', () => {
        cy.contains('button', EYPP_TILE_TEXT).should('exist');
    });
});

describe('Single check menu — Manchester City Council LA (WF enabled)', () => {
    beforeEach(() => {
        cy.loginManchesterLA();
        cy.visit('/home');
        cy.contains('a', 'Run a check').click();
        cy.url().should('include', '/Home/MenuSingleCheck');
    });

    it('does not show the "Childcare for working families" tile', () => {
        cy.contains('button', WF_TILE_TEXT).should('exist');
    });

    it('still shows the "Early learning for 2-year-olds" tile', () => {
        cy.contains('button', TWO_YO_TILE_TEXT).should('exist');
    });

    it('still shows the "Early years pupil premium" tile', () => {
        cy.contains('button', EYPP_TILE_TEXT).should('exist');
    });
});

describe('Batch check menu — Telford LA (WF disabled)', () => {
    beforeEach(() => {
        cy.loginLocalAuthorityUser();
        cy.visit('/home');
        cy.contains('a', 'Run a batch check').click();
        cy.url().should('include', '/Home/MenuBulkCheck');
    });

    it('shows the "Childcare for working families" tile', () => {
        cy.contains('button', WF_TILE_TEXT).should('not.exist');
    });

    it('shows the "Early learning for 2-year-olds" tile', () => {
        cy.contains('button', TWO_YO_TILE_TEXT).should('exist');
    });

    it('shows the "Early years pupil premium" tile', () => {
        cy.contains('button', EYPP_TILE_TEXT).should('exist');
    });
});

describe('Batch check menu — Manchester City Council LA (WF enabled)', () => {
    beforeEach(() => {
        cy.loginManchesterLA();
        cy.visit('/home');
        cy.contains('a', 'Run a batch check').click();
        cy.url().should('include', '/Home/MenuBulkCheck');
    });

    it('does not show the "Childcare for working families" tile', () => {
        cy.contains('button', WF_TILE_TEXT).should('exist');
    });

    it('still shows the "Early learning for 2-year-olds" tile', () => {
        cy.contains('button', TWO_YO_TILE_TEXT).should('exist');
    });

    it('still shows the "Early years pupil premium" tile', () => {
        cy.contains('button', EYPP_TILE_TEXT).should('exist');
    });
});
