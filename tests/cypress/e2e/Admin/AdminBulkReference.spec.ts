import "cypress-file-upload";

const services = [
  { key: "2YO", label: "Early learning for 2-year-olds", workingFamilies: false },
  { key: "EYPP", label: "Early years pupil premium", workingFamilies: false },
  { key: "WF", label: "Childcare for working families", workingFamilies: true },
];

services.forEach((service) => {
  const fields = service.workingFamilies
    ? ["Eligibility code", "National Insurance number", "Child date of birth"]
    : ["Parent Last Name", "Parent Date of Birth", "Parent National Insurance number"];
  const header = fields.concat("Reference (optional)").join(",");
  const row = service.workingFamilies
    ? "90012345671,AA123456C,2022-06-07"
    : "SIMPSON,2000-01-01,AB123456A";
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
          expect(response.headers["content-disposition"]).to.include(
            service.workingFamilies ? "WFBulkCheckTemplate.csv" : "BulkCheckTemplate.csv"
          );
        });
      });
    });

    it("identifies the missing Reference header even when reference values would be blank", () => {
      upload(fields.join(",") + "\r\n" + row, "missing-reference.csv");
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
      // Repeat the same eligibility inputs with different references to exercise cached-check behaviour.
      upload(header + "\r\n" + submittedReferences.map((reference) => row + "," + reference).join("\r\n"), fileName);
      cy.contains("h1", "Batch checks status", { timeout: 80000 }).should("be.visible");

      // The status page refreshes automatically. Scope the link to this upload, never a historical batch.
      cy.contains("tr", fileName, { timeout: 90000 })
        .contains("a", /Download results/i, { timeout: 90000 })
        .should("have.attr", "href").then((href) => {
          cy.request(String(href)).then((response) => {
            expect(response.status).to.eq(200);
            const lines = String(response.body).replace(/^\uFEFF/, "").split(/\r?\n/).filter((line) => line.length > 0);
            const expectedHeaders = service.workingFamilies
              ? fields.concat("Outcome", "Validity start date", "Validity end date", "Grace period ends", "Reference (optional)")
              : fields.concat("Outcome", "Reference (optional)");
            expect(lines[0]).to.eq(expectedHeaders.join(","));
            expect(lines).to.have.length(4);
            // This scenario uses plain CSV fields; quoted commas/newlines are covered by the NUnit export tests.
            lines.slice(1).forEach((line, index) => {
              const columns = line.split(",");
              expect(columns).to.have.length(expectedHeaders.length);
              expect(columns.slice(0, 3)).to.deep.eq(row.split(","));
              expect(columns[3]).not.to.eq("");
              expect(columns[columns.length - 1]).to.eq(expectedReferences[index]);
            });
          });
        });
    });
  });
});
