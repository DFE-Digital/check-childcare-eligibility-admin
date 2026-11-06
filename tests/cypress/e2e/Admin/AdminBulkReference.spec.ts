import "cypress-file-upload";

const services = [
  {
    key: "2YO",
    label: "Early learning for 2-year-olds",
    template: "BulkCheckTemplate.csv",
    uploadHeaders: [
      "Parent Last Name",
      "Parent Date of Birth",
      "Parent National Insurance number",
      "Reference (optional)",
    ],
    uploadRow: ["SIMPSON", "2000-01-01", "AB123456A"],
    resultHeaders: [
      "Parent Last Name",
      "Parent Date of Birth",
      "Parent National Insurance number",
      "Outcome",
      "Reference (optional)",
    ],
    resultRow: ["SIMPSON", "2000-01-01", "AB123456A"],
  },
  {
    key: "EYPP",
    label: "Early years pupil premium",
    template: "EYPPBulkCheckTemplate.csv",
    uploadHeaders: [
      "Parent Last Name",
      "Parent Date of Birth",
      "Parent National Insurance number",
      "Parent Asylum Seeker Reference number",
      "Reference (optional)",
    ],
    uploadRow: ["SIMPSON", "2000-01-01", "AB123456A", ""],
    resultHeaders: [
      "Parent Last Name",
      "Parent Date of Birth",
      "Parent National Insurance number",
      "Parent Asylum Seeker Reference number",
      "Outcome",
      "Reference (optional)",
    ],
    resultRow: ["SIMPSON", "2000-01-01", "AB123456A"],
  },
  {
    key: "WF",
    label: "Childcare for working families",
    template: "WFBulkCheckTemplate.csv",
    uploadHeaders: [
      "Eligibility code",
      "National Insurance number",
      "Child date of birth",
      "Reference (optional)",
    ],
    uploadRow: ["90012345671", "AA123456C", "2022-06-07"],
    resultHeaders: [
      "Eligibility code",
      "National Insurance number",
      "Child date of birth",
      "Outcome",
      "Validity start date",
      "Validity end date",
      "Grace period ends",
      "Reference (optional)",
    ],
    resultRow: ["90012345671", "AA123456C", "2022-06-07"],
  },
];

services.forEach((service) => {
  const header = service.uploadHeaders.join(",");
  const row = service.uploadRow.join(",");
  const guidance = "There is also an optional 'Reference' column where you can add reference numbers from your management system.";

  const upload = (content: string, fileName: string) => {
    cy.get('input[type="file"]').attachFile({ fileContent: content, fileName, mimeType: "text/csv" });
    cy.contains("button", "Run check").click();
  };

  describe(`${service.key} batch references`, () => {
    beforeEach(() => {
      cy.checkSession("manchesterLA");
      cy.visit("/home");
      cy.contains("Run a batch check").click();
      cy.contains("button", service.label).click();
    });

    it("shows reference guidance and downloads the current template under its existing filename", () => {
      cy.contains("p", guidance).should("be.visible");
      cy.contains("a", "batch check template").should("have.attr", "href").then((href) => {
        cy.request(String(href)).then((response) => {
          expect(response.status).to.eq(200);
          const firstLine = String(response.body).replace(/^\uFEFF/, "").split(/\r?\n/)[0];
          expect(firstLine).to.eq(header);
          expect(response.headers["content-disposition"]).to.include(service.template);
        });
      });
    });

    it("identifies the missing Reference header even when reference values would be blank", () => {
      upload(service.uploadHeaders.slice(0, -1).join(",") + "\r\n" + row, "missing-reference.csv");
      cy.get("#file-upload-1-error").should("contain.text",
        "The selected file must include the column heading Reference (optional).");
    });

    it("rejects a reference longer than 100 characters after trimming", () => {
      upload(header + "\r\n" + row + ",  " + "x".repeat(101) + "  ", "overlong-reference.csv");
      cy.contains("h1", "Fix data errors").should("be.visible");
      cy.contains("tbody tr", "Reference must be 100 characters or fewer.")
        .find("td").first().should("have.text", "2");
    });

    it("returns trimmed and blank references for the corresponding rows in the newly uploaded batch", () => {
      const expectedReferences = ["a".repeat(100), "", "MiXeD-003"];
      const submittedReferences = ["  " + expectedReferences[0] + "  ", "", "  MiXeD-003  "];
      const fileName = `${service.key}-references-${Date.now()}-${Cypress._.random(100000, 999999)}.csv`;
      upload(header + "\r\n" + submittedReferences.map((reference) => row + "," + reference).join("\r\n"), fileName);
      cy.contains("h1", "Batch checks status", { timeout: 80000 }).should("be.visible");

      cy.contains("tr", fileName, { timeout: 90000 })
        .contains("a", /Download results/i, { timeout: 90000 })
        .should("have.attr", "href").then((href) => {
          cy.request(String(href)).then((response) => {
            expect(response.status).to.eq(200);
            const lines = String(response.body).replace(/^\uFEFF/, "").split(/\r?\n/).filter((line) => line.length > 0);
            expect(lines[0]).to.eq(service.resultHeaders.join(","));
            expect(lines).to.have.length(4);
            lines.slice(1).forEach((line, index) => {
              const columns = line.split(",");
              expect(columns).to.have.length(service.resultHeaders.length);
              expect(columns.slice(0, service.resultRow.length)).to.deep.eq(service.resultRow);
              expect(columns[service.resultHeaders.indexOf("Outcome")]).not.to.eq("");
              expect(columns[columns.length - 1]).to.eq(expectedReferences[index]);
            });
          });
        });
    });
  });
});
